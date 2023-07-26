using System;
using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Nekres.ProofLogix.Core.Services.PartySync.Models;
using Nekres.ProofLogix.Core.UI.Configs;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Nekres.ProofLogix.Core.UI.KpProfile;

namespace Nekres.ProofLogix.Core.UI.Table {
    public class TablePresenter : Presenter<TableView, TableConfig> {

        public TablePresenter(TableView view, TableConfig model) : base(view, model) {
            ProofLogix.Instance.PartySync.PlayerAdded   += PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerChanged += PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerRemoved += PlayerRemoved;
        }

        protected override async Task<bool> Load(IProgress<string> progress) {
            foreach (var id in this.Model.ProfileIds) {
                var profile = await ProofLogix.Instance.KpWebApi.GetProfile(id);
                ProofLogix.Instance.PartySync.AddKpProfile(profile);
            }
            return await base.Load(progress);
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

            entry.LeftMouseButtonReleased += (_, _) => {
                ProofLogix.Instance.Resources.PlayMenuItemClick();
                ProfileView.Open(entry.Player.KpProfile);
            };

            entry.RightMouseButtonReleased += (_, _) => {
                if (player == ProofLogix.Instance.PartySync.LocalPlayer) {
                    return;
                }

                if (entry.Remember) {
                    GameService.Content.PlaySoundEffectByName("button-click");
                    this.Model.ProfileIds.Remove(entry.Player.KpProfile.Id);
                } else {
                    GameService.Content.PlaySoundEffectByName("color-change");
                    this.Model.ProfileIds.Add(entry.Player.KpProfile.Id);
                }
                entry.Remember = !entry.Remember;
            };

            entry.Remember = this.Model.ProfileIds.Any(id => id.Equals(entry.Player.KpProfile.Id)) || player == ProofLogix.Instance.PartySync.LocalPlayer;

            table.ContentResized += (_, e) => {
                entry.Width = e.CurrentRegion.Width;
            };

            SortEntries();
        }

        private void PlayerRemoved(object sender, ValueEventArgs<Player> e) {
            if (this.View.Table == null) {
                return;
            }

            if (TryGetPlayerEntry(e.Value, out var playerEntry)) {
                this.View.Table.RemoveChild(playerEntry);
            }
        }

        private void PlayerAddedOrChanged(object sender, ValueEventArgs<Player> e) {
            if (this.View.Table == null) {
                return;
            }
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
            var list = this.View.Table.Children.Cast<TablePlayerEntry>().ToList();
            list.Sort(Comparer);
            this.View.Table.GetPrivateField("_children").SetValue(this.View.Table, new ControlCollection<Control>(list));
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
