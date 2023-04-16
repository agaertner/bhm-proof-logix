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

        private async void OnPlayerCharacterNameChanged(object sender, ValueEventArgs<string> e) {
            await AddLocalPlayer();
        }

        private async void OnUserLocaleChanged(object sender, ValueEventArgs<CultureInfo> e) {
            foreach (var member in _members.Values) {
                // Reattach localized KP profiles.
                member.AttachProfile(await ProofLogix.Instance.KpWebApi.GetProfile(member.AccountName));
            }
        }

        public async Task InitSquad() {
            await AddLocalPlayer();

            // Squad will be empty until map change if ArcDps just got activated.
            foreach (var player in GameService.ArcDps.Common.PlayersInSquad.Values) {
                AddArcDpsAgent(player);
            }
        }

        private async Task AddLocalPlayer() {
            var name = GameService.Gw2Mumble.PlayerCharacter.Name;
            if (string.IsNullOrEmpty(name)) {
                return;
            }
            var self = this.AddKpProfile(await ProofLogix.Instance.KpWebApi.GetProfile(name, true));
            self.IsLocalPlayer = true;
            self.CharacterName = name;
        }

        private void AddArcDpsAgent(CommonFields.Player arcDpsPlayer) {
            RwLockUtil.AcquireWriteLock(_rwLock, ref _lockAcquired);
            try {

                var key = arcDpsPlayer.AccountName.ToLowerInvariant();

                if (_members.TryGetValue(key, out var member)) {

                    member.AttachAgent(arcDpsPlayer); // Overwrite player agent.
                    return;
                }

                member = Player.FromArcDps(arcDpsPlayer);
                _members.Add(key, member);
                return;

            } finally {
                RwLockUtil.ReleaseWriteLock(_rwLock, ref _lockAcquired, _lockReleased);
            }
        }

        public Player AddKpProfile(Profile kpProfile) {
            RwLockUtil.AcquireWriteLock(_rwLock, ref _lockAcquired);
            try {

                var key = kpProfile.Name.ToLowerInvariant();

                if (_members.TryGetValue(key, out var member)) {

                    member.AttachProfile(kpProfile); // Overwrite KP profile.
                    return member;
                }

                member = Player.FromKpProfile(kpProfile);
                _members.Add(key, member);
                return member;

            } finally {
                RwLockUtil.ReleaseWriteLock(_rwLock, ref _lockAcquired, _lockReleased);
            }
        }

        public void RemovePlayer(string accountName) {
            RwLockUtil.AcquireWriteLock(_rwLock, ref _lockAcquired);
            try {
                _members.Remove(accountName.ToLowerInvariant());
            } finally {
                RwLockUtil.ReleaseWriteLock(_rwLock, ref _lockAcquired, _lockReleased);
            }
        }

        #region ArcDps Player Events
        private async void OnPlayerAdded(CommonFields.Player player) {
            this.AddArcDpsAgent(player);
            this.AddKpProfile(await ProofLogix.Instance.KpWebApi.GetProfile(player.AccountName));
        }

        private void OnPlayerRemoved(CommonFields.Player player) {
            if (player.Self) {
                return; // Never remove local player.
            }
            this.RemovePlayer(player.AccountName);
        }
        #endregion

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
    }
}
