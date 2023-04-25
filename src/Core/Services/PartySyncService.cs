using Blish_HUD;
using Blish_HUD.ArcDps.Common;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using Nekres.ProofLogix.Core.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Player = Nekres.ProofLogix.Core.Services.PartySync.Models.Player;

namespace Nekres.ProofLogix.Core.Services {
    internal class PartySyncService : IDisposable {

        public static event EventHandler<ValueEventArgs<Player>> OnPlayerAdded;
        public static event EventHandler<ValueEventArgs<Player>> OnPlayerRemoved;
        public static event EventHandler<ValueEventArgs<Player>> OnPlayerChanged;

        public static IReadOnlyList<Player> PlayerList => _members.Values.ToList();

        private static   Dictionary<string, Player> _members      = new();
        private readonly ReaderWriterLockSlim       _rwLock       = new();
        private          ManualResetEvent           _lockReleased = new(false);
        private          bool                       _lockAcquired = false;

        public PartySyncService() {
            GameService.Gw2Mumble.PlayerCharacter.NameChanged += OnPlayerCharacterNameChanged;

            GameService.ArcDps.Common.PlayerAdded   += OnPlayerJoin;
            GameService.ArcDps.Common.PlayerRemoved += OnPlayerLeft;

            GameService.Overlay.UserLocaleChanged += OnUserLocaleChanged;
        }

        /// <summary>
        /// Initializes all players currently in the party.
        /// </summary>
        public async Task InitSquad() {
            await AddLocalPlayer();

            // Squad will be empty until map change if ArcDps just got activated.
            foreach (var player in GameService.ArcDps.Common.PlayersInSquad.Values) {
                AddArcDpsAgent(player);
            }
        }

        /// <summary>
        /// Adds a player by a given <see href="https://www.killproof.me/">www.killproof.me</see> profile to the list of available players.
        /// </summary>
        /// <param name="kpProfile">Profile to add.</param>
        public void AddKpProfile(Profile kpProfile) => AddKpProfile(kpProfile, false);

        /// <summary>
        /// Removes a player by a given account name.
        /// </summary>
        /// <param name="accountName">Account to remove.</param>
        public void RemovePlayer(string accountName) {
            RwLockUtil.AcquireWriteLock(_rwLock, ref _lockAcquired);
            try { 
                var key = accountName.ToLowerInvariant();

                if (!_members.TryGetValue(key, out var member) || member.IsLocalPlayer || !_members.Remove(key)) {
                    return;
                }

                OnPlayerRemoved?.Invoke(this, new ValueEventArgs<Player>(member));

            } finally {
                RwLockUtil.ReleaseWriteLock(_rwLock, ref _lockAcquired, _lockReleased);
            }
        }

        /// <summary>
        /// Disposes the <see cref="PartySyncService"/> and frees all its held resources.
        /// </summary>
        public void Dispose() {
            GameService.Gw2Mumble.PlayerCharacter.NameChanged -= OnPlayerCharacterNameChanged;
            GameService.Overlay.UserLocaleChanged             -= OnUserLocaleChanged;
            GameService.ArcDps.Common.PlayerAdded             -= OnPlayerJoin;
            GameService.ArcDps.Common.PlayerRemoved           -= OnPlayerLeft;

            OnPlayerAdded = null;
            OnPlayerRemoved = null;

            RwLockUtil.Dispose(_rwLock, ref _lockAcquired, _lockReleased);

            _members = null;
        }

        private async void OnPlayerCharacterNameChanged(object sender, ValueEventArgs<string> e) {
            await AddLocalPlayer();
        }

        private async void OnUserLocaleChanged(object sender, ValueEventArgs<CultureInfo> e) {
            foreach (var member in _members.Values) {
                // Reattach localized KP profiles.
                member.AttachProfile(await ProofLogix.Instance.KpWebApi.GetProfile(member.AccountName));
                OnPlayerChanged?.Invoke(this, new ValueEventArgs<Player>(member));
            }
        }

        private async Task AddLocalPlayer() {
            var profile = await ProofLogix.Instance.KpWebApi.GetProfile(GameService.Gw2Mumble.PlayerCharacter.Name, true);
            AddKpProfile(profile, true);
        }

        private void AddArcDpsAgent(CommonFields.Player arcDpsPlayer) {
            if (string.IsNullOrEmpty(arcDpsPlayer.AccountName)) {
                return; // No account name to use as key.
            }

            RwLockUtil.AcquireWriteLock(_rwLock, ref _lockAcquired);
            try {

                var key = arcDpsPlayer.AccountName.ToLowerInvariant();

                if (_members.TryGetValue(key, out var member)) {

                    member.AttachAgent(arcDpsPlayer); // Overwrite player agent.

                    OnPlayerChanged?.Invoke(this, new ValueEventArgs<Player>(member));

                    return;
                }

                member = Player.FromArcDps(arcDpsPlayer);
                _members.Add(key, member);

                OnPlayerAdded?.Invoke(this, new ValueEventArgs<Player>(member));

            } finally {
                RwLockUtil.ReleaseWriteLock(_rwLock, ref _lockAcquired, _lockReleased);
            }
        }

        private void AddKpProfile(Profile kpProfile, bool isLocalPlayer) {
            if (kpProfile.IsEmpty) {
                return; // No account name to use as key.
            }

            RwLockUtil.AcquireWriteLock(_rwLock, ref _lockAcquired);
            try {

                var key = kpProfile.Name.ToLowerInvariant();

                if (_members.TryGetValue(key, out var member)) {

                    member.AttachProfile(kpProfile, isLocalPlayer); // Overwrite KP profile.

                    OnPlayerChanged?.Invoke(this, new ValueEventArgs<Player>(member));

                    return;
                }

                member = Player.FromKpProfile(kpProfile, isLocalPlayer);
                _members.Add(key, member);

                OnPlayerAdded?.Invoke(this, new ValueEventArgs<Player>(member));

            } finally {
                RwLockUtil.ReleaseWriteLock(_rwLock, ref _lockAcquired, _lockReleased);
            }
        }

        #region ArcDps Player Events
        private async void OnPlayerJoin(CommonFields.Player player) {
            AddArcDpsAgent(player);
            AddKpProfile(await ProofLogix.Instance.KpWebApi.GetProfile(player.AccountName), player.Self);
        }

        private void OnPlayerLeft(CommonFields.Player player) {
            if (player.Self) {
                return; // Never remove local player.
            }
            RemovePlayer(player.AccountName);
        }
        #endregion
    }
}
