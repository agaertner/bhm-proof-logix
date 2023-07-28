using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Nekres.ProofLogix.Core.Services.PartySync.Models;
using Nekres.ProofLogix.Core.UI.Configs;
using Nekres.ProofLogix.Core.UI.KpProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Nekres.ProofLogix.Core.UI.Table {
    public class TablePresenter : Presenter<TableView, TableConfig> {

        private readonly Timer _bulkLoadTimer;

        private const int BULKLOAD_INTERVAL = 1000;

        private readonly SynchronizedCollection<TablePlayerEntry> _bulk;

        public TablePresenter(TableView view, TableConfig model) : base(view, model) {
            _bulk = new SynchronizedCollection<TablePlayerEntry>();
            _bulkLoadTimer         =  new Timer(1000) { AutoReset = false };
            _bulkLoadTimer.Elapsed += OnBulkLoadTimerElapsed;

            ProofLogix.Instance.PartySync.PlayerAdded   += PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerChanged += PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerRemoved += PlayerRemoved;
        }

        private void OnBulkLoadTimerElapsed(object sender, ElapsedEventArgs e) {
            var table = this.View.Table;
            if (table == null) {
                return;
            }

            // Bulk assign children to container.
            // Prepare sorted control collection.
            var bulk = _bulk.ToList();
            bulk.Sort(Comparer);
            var list = new ControlCollection<Control>(bulk);

            // Assign parent on each child since AddChild (which would assign it) is skipped below.
            foreach (var item in list) {
                // Skip the public setter because it would add to the parent's children individually.
                item.GetPrivateField("_parent").SetValue(item, table);
            }
            
            // Overwrite the old list of children as a whole.
            table.GetPrivateField("_children").SetValue(table, list);

            table.Invalidate();
        }

        protected override async Task<bool> Load(IProgress<string> progress) {
            foreach (var id in this.Model.ProfileIds) {
                var profile = await ProofLogix.Instance.KpWebApi.GetProfile(id);
                ProofLogix.Instance.PartySync.AddKpProfile(profile);
            }
            return await base.Load(progress);
        }

        public void CreatePlayerEntry(Player player) {
            var table = this.View.Table;
            if (table == null) {
                return;
            }

            if (TryGetPlayerEntry(player, out var playerEntry)) {
                playerEntry.Player = player; // Reassign just in case it's a new player.
                return;
            }

            if (!player.HasKpProfile) {
                return;
            }

            _bulkLoadTimer.Stop();

            var entry = new TablePlayerEntry(player) {
                Width = table.ContentRegion.Width,
                Height = 32,
                Remember = this.Model.ProfileIds.Any(id => id.Equals(player.KpProfile.Id))
                        || player.Equals(ProofLogix.Instance.PartySync.LocalPlayer)
            };
            
            _bulk.Add(entry);
            _bulkLoadTimer.Interval = BULKLOAD_INTERVAL;
            _bulkLoadTimer.Start();

            entry.LeftMouseButtonReleased += (_, _) => {
                ProofLogix.Instance.Resources.PlayMenuItemClick();
                ProfileView.Open(entry.Player.KpProfile);
            };

            entry.RightMouseButtonReleased += (_, _) => {
                if (player.Equals(ProofLogix.Instance.PartySync.LocalPlayer)) {
                    return;
                }
                if (entry.Remember) {
                    GameService.Content.PlaySoundEffectByName("button-click");
                    this.Model.ProfileIds.RemoveAll(entry.Player.KpProfile.Id);
                } else {
                    GameService.Content.PlaySoundEffectByName("color-change");
                    this.Model.ProfileIds.Add(entry.Player.KpProfile.Id);
                }
                entry.Remember = !entry.Remember;
            };

            table.ContentResized += (_, e) => {
                entry.Width = e.CurrentRegion.Width;
            };
        }

        private void PlayerRemoved(object sender, ValueEventArgs<Player> e) {
            if (!TryGetPlayerEntry(e.Value, out var playerEntry)) {
                return;
            }

            if (playerEntry.Remember) {
                return; // Don't remove remembered entries.
            }

            _bulkLoadTimer.Stop();

            if (_bulk.Remove(playerEntry)) {
                playerEntry.Dispose();
            }

            _bulkLoadTimer.Interval = BULKLOAD_INTERVAL;
            _bulkLoadTimer.Start();
        }

        private void PlayerAddedOrChanged(object sender, ValueEventArgs<Player> e) {
            CreatePlayerEntry(e.Value);
        }

        private bool TryGetPlayerEntry(Player player, out TablePlayerEntry playerEntry) {
            playerEntry = _bulk.FirstOrDefault(ctrl => ctrl.Player.Equals(player));
            return playerEntry != null;
        }

        private int Comparer(TablePlayerEntry x, TablePlayerEntry y) {
            var column = this.Model.SelectedColumn;
            var comparison = 0;

            if (column == (int)TableConfig.Column.Timestamp) {
                comparison = x.Player.Created.CompareTo(y.Player.Created);
            }

            if (column == (int)TableConfig.Column.Class) {
                comparison = string.Compare(x.Player.Class, y.Player.Class, StringComparison.InvariantCultureIgnoreCase);
            }

            if (column == (int)TableConfig.Column.Character) {
                comparison = string.Compare(x.Player.CharacterName, y.Player.CharacterName, StringComparison.InvariantCultureIgnoreCase);
            }

            if (column == (int)TableConfig.Column.Account) {
                comparison = string.Compare(x.Player.AccountName, y.Player.AccountName, StringComparison.InvariantCultureIgnoreCase);
            }

            // All trailing columns are known to be tokens.
            var len = Enum.GetValues(typeof(TableConfig.Column)).Length;
            if (column >= len) {
                var id = ProofLogix.Instance.TableConfig.Value.TokenIds.ElementAtOrDefault(column - len);
                comparison = x.Player.KpProfile.GetToken(id).Amount.CompareTo(y.Player.KpProfile.GetToken(id).Amount);
            }
            return this.Model.OrderDescending ? comparison : -comparison;
        }

        protected override void Unload() {
            ProofLogix.Instance.PartySync.PlayerAdded   -= PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerChanged -= PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerRemoved -= PlayerRemoved;

            _bulkLoadTimer.Dispose();

            foreach (var ctrl in _bulk) {
                ctrl?.Dispose();
            }

            base.Unload();
        }

        public void SortEntries() {
            this.View.Table.SortChildren<TablePlayerEntry>(Comparer);
        }

    }
}
