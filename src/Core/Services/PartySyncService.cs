using Blish_HUD;
using Blish_HUD.ArcDps.Common;
using Microsoft.Xna.Framework;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using Nekres.ProofLogix.Core.Services.PartySync.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Player = Nekres.ProofLogix.Core.Services.PartySync.Models.Player;

namespace Nekres.ProofLogix.Core.Services {
    public class PartySyncService : IDisposable {

        public event EventHandler<ValueEventArgs<Player>> PlayerAdded;
        public event EventHandler<ValueEventArgs<Player>> PlayerRemoved;
        public event EventHandler<ValueEventArgs<Player>> PlayerChanged;

        public readonly MumblePlayer LocalPlayer;

        public IReadOnlyList<Player> PlayerList  => _members.Values.ToList();

        private readonly ConcurrentDictionary<string, Player> _members;

        private readonly Timer _overcapCleanUpTimer;

        private readonly Color _redShift = new(255, 57, 57);

        private readonly Color _unknownColor = new(127, 128, 127);

        private readonly Color _awayColor = new(255, 165, 0);

        private readonly Color _onlineColor = new(0, 255, 0);

        public enum ColorGradingMode {
            LocalPlayerComparison,
            MedianComparison,
            LargestComparison,
            AverageComparison
            //CustomComparison
        }

        public PartySyncService() {
            this.LocalPlayer = new MumblePlayer();

            _members           = new ConcurrentDictionary<string, Player>();
            _overcapCleanUpTimer = new Timer(HandleOvercap, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

            GameService.Gw2Mumble.PlayerCharacter.NameChanged += OnPlayerCharacterNameChanged;

            GameService.ArcDps.Common.PlayerAdded   += OnPlayerJoin;
            GameService.ArcDps.Common.PlayerRemoved += OnPlayerLeft;

            GameService.Overlay.UserLocaleChanged += OnUserLocaleChanged;
        }

        public Color GetTokenAmountColor(int id, int amount, ColorGradingMode gradingMode) {
            float maxAmount = amount;
            switch (gradingMode) {
                case ColorGradingMode.LocalPlayerComparison:
                    maxAmount = this.LocalPlayer.KpProfile.GetToken(id).Amount;
                    break;
                case ColorGradingMode.MedianComparison:
                    // Players can have wildly different amounts and as such the distribution can be far from symmetrically.
                    // We use median here because extreme outliers don't affect it.
                    maxAmount = _members.Count > 0 ? (float)_members.Values.Median(member => member.KpProfile.GetToken(id).Amount) : amount;
                    break;
                case ColorGradingMode.LargestComparison:
                    maxAmount = GetLargestAmount(id);
                    break;
                case ColorGradingMode.AverageComparison:
                    maxAmount = _members.Count > 0 ? (float)_members.Values.Average(member => member.KpProfile.GetToken(id).Amount) : amount;
                    break;
            }
            var diff = maxAmount - amount;
            return diff <= 0 ? Color.White : Color.Lerp(Color.White, _redShift, diff / maxAmount);
        }

        private int GetLargestAmount(int id) {
            return _members.Count > 0 ? _members.Values.Max(x => x.KpProfile.GetToken(id).Amount) : this.LocalPlayer.KpProfile.GetToken(id).Amount;
        }

        public Color GetStatusColor(Player.OnlineStatus status) {
            return status switch {
                Player.OnlineStatus.Unknown => _unknownColor,
                Player.OnlineStatus.Away    => _awayColor,
                Player.OnlineStatus.Online  => _onlineColor,
                _                           => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        /// Initializes all players currently in the party.
        /// </summary>
        public async Task InitSquad() {
            await GetLocalPlayerProfile();

            // Squad will be empty until map change if ArcDps just got activated.
            foreach (var player in GameService.ArcDps.Common.PlayersInSquad.Values) {
                AddArcDpsAgent(player);
            }

            foreach (var id in ProofLogix.Instance.TableConfig.Value.ProfileIds) {
                var profile = await ProofLogix.Instance.KpWebApi.GetProfile(id);
                AddKpProfile(profile);
            }
        }

        /// <summary>
        /// Removes a player by a given account name.
        /// </summary>
        /// <param name="accountName">Account to remove.</param>
        public void RemovePlayer(string accountName) {
            if (string.IsNullOrEmpty(accountName)) {
                return;
            }

            var key = accountName.ToLowerInvariant();

            Player member;

            if (ProofLogix.Instance.TableConfig.Value.KeepLeavers) {
                if (_members.TryGetValue(key, out member)) {
                    member.Status = Player.OnlineStatus.Away;
                }
                return;
            }

            if (!_members.TryRemove(key, out member)) {
                return;
            }

            member.Status = Player.OnlineStatus.Away;
            PlayerRemoved?.Invoke(this, new ValueEventArgs<Player>(member));
        }

        /// <summary>
        /// Disposes the <see cref="PartySyncService"/> and frees all its held resources.
        /// </summary>
        public void Dispose() {
            GameService.Gw2Mumble.PlayerCharacter.NameChanged -= OnPlayerCharacterNameChanged;
            GameService.Overlay.UserLocaleChanged             -= OnUserLocaleChanged;
            GameService.ArcDps.Common.PlayerAdded             -= OnPlayerJoin;
            GameService.ArcDps.Common.PlayerRemoved           -= OnPlayerLeft;
            _overcapCleanUpTimer?.Dispose();
        }

        public void AddKpProfile(Profile kpProfile) {

            var key = kpProfile.Name;

            if (string.IsNullOrWhiteSpace(key)) {
                return; // No account name to use as key.
            }

            if (HasAccountInParty(key, out var existingAccount)) {
                key = existingAccount;
            };

            if (this.LocalPlayer.HasKpProfile && this.LocalPlayer.KpProfile.BelongsTo(key, out _) || 
                 this.LocalPlayer.AccountName.ToLowerInvariant().Equals(key.ToLowerInvariant())) {
                this.LocalPlayer.AttachProfile(kpProfile);
                PlayerChanged?.Invoke(this, new ValueEventArgs<Player>(this.LocalPlayer));
                return;
            }

            var member = _members.AddOrUpdate(key.ToLowerInvariant(), _ => {

                var member = new Player(kpProfile);
                PlayerAdded?.Invoke(this, new ValueEventArgs<Player>(member));
                return member;

            }, (_, member) => {

                member.AttachProfile(kpProfile); // Overwrite KP profile.
                PlayerChanged?.Invoke(this, new ValueEventArgs<Player>(member));
                return member;

            });
        }

        private async void OnPlayerCharacterNameChanged(object sender, ValueEventArgs<string> e) {
            // In cases were mumble had no data when blish started (never went past character select)
            // pull the profile now.
            await GetLocalPlayerProfile();
        }

        private async void OnUserLocaleChanged(object sender, ValueEventArgs<CultureInfo> e) {
            foreach (var member in _members.Values) {
                // Reattach localized KP profiles.
                member.AttachProfile(await ProofLogix.Instance.KpWebApi.GetProfile(member.AccountName));
                PlayerChanged?.Invoke(this, new ValueEventArgs<Player>(member));
            }
        }

        private async Task GetLocalPlayerProfile() {
            var profile = await ProofLogix.Instance.KpWebApi.GetProfileByCharacter(GameService.Gw2Mumble.PlayerCharacter.Name);
            this.LocalPlayer.AttachProfile(profile);
        }

        private void AddArcDpsAgent(CommonFields.Player arcDpsPlayer) {
            var key = arcDpsPlayer.AccountName;

            if (string.IsNullOrEmpty(key)) {
                return; // No account name to use as key.
            }

            if (HasAccountInParty(key, out var existingAccount)) {
                key = existingAccount;
            };

            if (arcDpsPlayer.Self) {
                this.LocalPlayer.AttachAgent(arcDpsPlayer);
                PlayerChanged?.Invoke(this, new ValueEventArgs<Player>(this.LocalPlayer));
                return;
            }

            var member = _members.AddOrUpdate(key.ToLowerInvariant(), _ => {
                var member = new Player(arcDpsPlayer);
                PlayerAdded?.Invoke(this, new ValueEventArgs<Player>(member));
                return member;
            }, (_, member) => {
                member.AttachAgent(arcDpsPlayer); // Overwrite player agent.
                PlayerChanged?.Invoke(this, new ValueEventArgs<Player>(member));
                return member;
            });

            member.Status = Player.OnlineStatus.Online;
        }

        private bool HasAccountInParty(string account, out string existingAccount) {
            var existingMember = _members.Values.FirstOrDefault(member => member.HasKpProfile && member.KpProfile.BelongsTo(account, out _));

            if (existingMember != null) {
                existingAccount = existingMember.AccountName;
                return true;
            }

            existingAccount = string.Empty;
            return false;
        }

        #region ArcDps Player Events
        private async void OnPlayerJoin(CommonFields.Player player) {
            AddArcDpsAgent(player);
            AddKpProfile(await ProofLogix.Instance.KpWebApi.GetProfile(player.AccountName));
        }

        private void OnPlayerLeft(CommonFields.Player player) {
            if (player.Self) {
                return; // Never remove local player.
            }
            RemovePlayer(player.AccountName);
        }
        #endregion

        // Handle overcap in a separate thread, periodically checking if we exceed the player limit
        private void HandleOvercap(object state) {
            var count          = _members.Count;
            var maxPlayerCount = Math.Abs(ProofLogix.Instance.TableConfig.Value.MaxPlayerCount - 1);
            if (count > maxPlayerCount) {
                var oldMembers = _members
                                     .Where(kv => kv.Value.Status < Player.OnlineStatus.Online)
                                     .OrderBy(kv => kv.Value.Created)
                                     .Select(kv => kv.Key).Take(Math.Abs(count - maxPlayerCount));
                foreach (var member in oldMembers) {
                    _members.TryRemove(member, out _);
                }
            }
        }
    }
}
