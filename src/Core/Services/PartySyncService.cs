using Blish_HUD;
using Blish_HUD.ArcDps.Common;
using Blish_HUD.Content;
using Blish_HUD.Extended;
using Gw2Sharp.Models;
using Nekres.ProofLogix.Core.Services.PartySync.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Blish_HUD.ArcDps.Common.CommonFields;
using Player = Nekres.ProofLogix.Core.Services.PartySync.Models.Player;

namespace Nekres.ProofLogix.Core.Services {
    internal class PartySyncService : IDisposable {
        public static Dictionary<int, string>         ProfNames  { get; private set; }
        public static Dictionary<int, AsyncTexture2D> ProfIcons  { get; private set; }
        public static Dictionary<int, string>         EliteNames { get; private set; }
        public static Dictionary<int, AsyncTexture2D> EliteIcons { get; private set; }

        private          Dictionary<string, Player> _members      = new();
        private readonly ReaderWriterLockSlim       _rwLock       = new();
        private          ManualResetEvent           _lockReleased = new(false);
        private          bool                       _lockAcquired = false;

        public PartySyncService() {
            GameService.ArcDps.Common.PlayerAdded   += OnPlayerAdded;
            GameService.ArcDps.Common.PlayerRemoved += OnPlayerRemoved;

            GameService.Overlay.UserLocaleChanged += OnUserLocaleChanged;
        }

        private async void OnUserLocaleChanged(object sender, ValueEventArgs<CultureInfo> e) {
            await LoadAsync();

            foreach (var member in _members.Values) {
                await member.LoadAsync();
            }
        }

        public async Task LoadAsync() {
            await LoadResources();

            foreach (var player in GameService.ArcDps.Common.PlayersInSquad.Values) {
                await AddArcDpsAgent(player);
            }
        }

        private async Task LoadResources() {
            var professions = await TaskUtil.RetryAsync(() => GameService.Gw2WebApi.AnonymousConnection.Client.V2.Professions.AllAsync());

            if (professions == null) {
                return;
            }

            var specializations = await TaskUtil.RetryAsync(() => GameService.Gw2WebApi.AnonymousConnection.Client.V2.Specializations.AllAsync());

            if (specializations == null) {
                return;
            }

            var elites = specializations.Where(x => x.Elite).ToList();

            ProfNames = professions.ToDictionary(x => (int) (ProfessionType) Enum.Parse(typeof(ProfessionType), x.Id, true), x => x.Name);

            ProfIcons = professions.ToDictionary(x => (int) (ProfessionType) Enum.Parse(typeof(ProfessionType), x.Id, true), x => GameService.Content.GetRenderServiceTexture(x.IconBig.ToString()));

            EliteNames = elites.ToDictionary(x => x.Id, x => x.Name);

            EliteIcons = elites.ToDictionary(x => x.Id, x => GameService.Content.GetRenderServiceTexture(x.ProfessionIconBig.ToString()));
        }

        private async Task AddArcDpsAgent(CommonFields.Player arcDpsPlayer) {

            var key = arcDpsPlayer.AccountName.ToLowerInvariant();

            if (_members.TryGetValue(key, out var member)) {

                member.AttachAgent(arcDpsPlayer); // Attach the new player agent.

                return;
            }

            member = Player.FromArcDps(arcDpsPlayer);
            _members.Add(key, member);

            await member.LoadAsync();
        }

        #region ArcDps Player Events
        private async void OnPlayerAdded(CommonFields.Player player) {
            this.AcquireWriteLock();
            try {
                await this.AddArcDpsAgent(player);
            } finally {
                this.ReleaseWriteLock();
            }
        }

        private void OnPlayerRemoved(CommonFields.Player player) {
            try {
                _members.Remove(player.AccountName.ToLowerInvariant());
            } finally {
                this.ReleaseWriteLock();
            }
        }
        #endregion

        public async Task<bool> TryAddManually(string accountName) {
            try {
                var key = accountName.ToLowerInvariant();

                if (_members.ContainsKey(key)) {
                    return false;
                }

                var member = new Player(accountName);
                _members.Add(key, member);

                await member.LoadAsync();

                return true;

            } finally {
                this.ReleaseWriteLock();
            }
        }

        private void AcquireWriteLock() {
            try {
                _rwLock.EnterWriteLock();
                _lockAcquired = true;
            } catch (Exception ex) {
                ProofLogix.Logger.Debug(ex, ex.Message);
            }
        }

        private void ReleaseWriteLock() {
            try {
                if (_lockAcquired) {
                    _rwLock.ExitWriteLock();
                    _lockAcquired = false;
                }
            } catch (Exception ex) {
                ProofLogix.Logger.Debug(ex, ex.Message);
            } finally {
                _lockReleased.Set();
            }
        }

        public void Dispose() {
            GameService.Overlay.UserLocaleChanged   -= OnUserLocaleChanged;
            GameService.ArcDps.Common.PlayerAdded   -= OnPlayerAdded;
            GameService.ArcDps.Common.PlayerRemoved -= OnPlayerRemoved;

            EliteNames = null;
            ProfNames  = null;

            if (EliteIcons != null) {
                foreach (var icon in EliteIcons.Values) {
                    icon?.Dispose();
                }
                EliteIcons = null;
            }

            if (ProfIcons != null) {
                foreach (var icon in ProfIcons.Values) {
                    icon?.Dispose();
                }
                ProfIcons = null;
            }

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
