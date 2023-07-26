using System;
using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Nekres.ProofLogix.Core.Services.PartySync.Models;
using Nekres.ProofLogix.Core.UI.Configs;
using System.Linq;
using Blish_HUD.Controls;

namespace Nekres.ProofLogix.Core.UI.Table {
    public class TablePresenter : Presenter<TableView, TableConfig> {

        public TablePresenter(TableView view, TableConfig model) : base(view, model) {
            ProofLogix.Instance.PartySync.PlayerAdded   += PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerChanged += PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerRemoved += PlayerRemoved;
        }

        public void AddPlayer(Player player) {
            if (TryGetPlayerEntry(player, out var playerEntry)) {
                playerEntry.Player = player;
                return;
            }

            if (!player.HasKpProfile) {
                return;
            }

            var table = this.View.Table;

            var entry = new TablePlayerEntry(player) {
                Parent = table,
                Width = table.ContentRegion.Width,
                Height = 32
            };

            table.ContentResized += (_, e) => {
                entry.Width = e.CurrentRegion.Width;
            };

            SortEntries();
        }

        private void PlayerRemoved(object sender, ValueEventArgs<Player> e) {
            if (TryGetPlayerEntry(e.Value, out var playerEntry)) {
                this.View.Table.RemoveChild(playerEntry);
            }
        }

        private void PlayerAddedOrChanged(object sender, ValueEventArgs<Player> e) {
            AddPlayer(e.Value);
        }

        private bool TryGetPlayerEntry(Player player, out TablePlayerEntry playerEntry) {
            var key = player.AccountName?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(key)) {
                playerEntry = null;
                return false;
            }
            playerEntry = this.View.Table.GetDescendants().FirstOrDefault(x => x is TablePlayerEntry ctrl && ctrl.Player.AccountName.ToLowerInvariant().Equals(key)) 
                              as TablePlayerEntry;
            return playerEntry != null;
        }

        public void SortEntries() {
            //TODO: Consider fixing the header at the top (excluding it from the FlowPanel)
            // Skip first entry (header) and sort the rest.
            var header = this.View.Table.Children.First();
            var list   = this.View.Table.Children.Skip(1).Cast<TablePlayerEntry>().ToList();
            list.Sort(Comparer);
            // Prepend header again and attach sorted list.
            this.View.Table.GetPrivateField("_children").SetValue(this.View.Table, new ControlCollection<Control>(list.Prepend(header)));
            this.View.Table.Invalidate();
        }

        private int Comparer(TablePlayerEntry x, TablePlayerEntry y) {
            var column     = this.Model.SelectedColumn;
            var comparison = 0;
            if (column == 0) {
                comparison = x.Player.Created.CompareTo(y.Player.Created);
            }

            if (column == 1) {
                comparison = string.Compare(x.Player.Class, y.Player.Class, StringComparison.InvariantCultureIgnoreCase);
            }

            if (column == 2) {
                comparison = string.Compare(x.Player.CharacterName, y.Player.CharacterName, StringComparison.InvariantCultureIgnoreCase);
            }

            if (column == 3) {
                comparison = string.Compare(x.Player.AccountName, y.Player.AccountName, StringComparison.InvariantCultureIgnoreCase);
            }

            if (column >= 4) {
                var id = ProofLogix.Instance.TableConfig.Value.TokenIds[column - 4];
                comparison = x.Player.KpProfile.GetToken(id).Amount.CompareTo(y.Player.KpProfile.GetToken(id).Amount);
            }
            return this.Model.OrderDescending ? comparison : -comparison;
        }

        protected override void Unload() {
            ProofLogix.Instance.PartySync.PlayerAdded   -= PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerChanged -= PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerRemoved -= PlayerRemoved;
            base.Unload();
        }
    }
}
