using Blish_HUD;
using Blish_HUD.ArcDps.Common;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using Nekres.ProofLogix.Core.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Player = Nekres.ProofLogix.Core.Services.PartySync.Models.Player;

namespace Nekres.ProofLogix.Core.Services {
    internal class PartySyncService : IDisposable {

        private          Dictionary<string, Player> _members      = new();
        private readonly ReaderWriterLockSlim       _rwLock       = new();
        private          ManualResetEvent           _lockReleased = new(false);
        private          bool                       _lockAcquired = false;

        public PartySyncService() {
            GameService.Gw2Mumble.PlayerCharacter.NameChanged += OnPlayerCharacterNameChanged;

            GameService.ArcDps.Common.PlayerAdded   += OnPlayerAdded;
            GameService.ArcDps.Common.PlayerRemoved += OnPlayerRemoved;

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

                if (!_members.TryGetValue(key, out var member) || member.IsLocalPlayer) {
                    return;
                }

                _members.Remove(key);
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
            GameService.ArcDps.Common.PlayerAdded             -= OnPlayerAdded;
            GameService.ArcDps.Common.PlayerRemoved           -= OnPlayerRemoved;

            // Wait for the lock to be released
            if (_lockAcquired) {
                _lockReleased.WaitOne(500);
            }

            _lockReleased.Dispose();

            // Dispose the lock
            try {
                _rwLock.Dispose();
            } catch (Exception ex) {
                ProofLogix.Logger.Debug(ex, ex.Message);
            }
        }

        private async void OnPlayerCharacterNameChanged(object sender, ValueEventArgs<string> e) {
            await AddLocalPlayer();
        }

        private async void OnUserLocaleChanged(object sender, ValueEventArgs<CultureInfo> e) {
            foreach (var member in _members.Values) {
                // Reattach localized KP profiles.
                member.AttachProfile(await ProofLogix.Instance.KpWebApi.GetProfile(member.AccountName));
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
                    return;
                }

                member = Player.FromArcDps(arcDpsPlayer);
                _members.Add(key, member);

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
                    return;
                }

                member = Player.FromKpProfile(kpProfile, isLocalPlayer);
                _members.Add(key, member);

            } finally {
                RwLockUtil.ReleaseWriteLock(_rwLock, ref _lockAcquired, _lockReleased);
            }
        }

        #region ArcDps Player Events
        private async void OnPlayerAdded(CommonFields.Player player) {
            AddArcDpsAgent(player);
            AddKpProfile(await ProofLogix.Instance.KpWebApi.GetProfile(player.AccountName), player.Self);
        }

        private void OnPlayerRemoved(CommonFields.Player player) {
            if (player.Self) {
                return; // Never remove local player.
            }
            RemovePlayer(player.AccountName);
        }
        #endregion
    }
}
