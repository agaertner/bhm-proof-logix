using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Nekres.ProofLogix.Core.Services.PartySync.Models;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.ProofLogix.Core.UI.Table {
    public class TablePresenter : Presenter<TableView, TableConfig> {

        public TablePresenter(TableView view, TableConfig model) : base(view, model) {
            ProofLogix.Instance.PartySync.PlayerAdded   += PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerChanged += PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerRemoved += PlayerRemoved;
        }

        public void AddPlayer(Player player) {
            var key = player.AccountName?.ToLowerInvariant();

            if (!player.HasKpProfile || string.IsNullOrEmpty(key)) {
                return;
            }

            if (player.KpProfile.NotFound) {
                return;
            }

            var size = LabelUtil.GetLabelSize(this.View.Table.Font, player.AccountName, true);
            var accountName = new FormattedLabelBuilder()
                       .SetWidth(size.X).SetHeight(size.Y)
                       .SetHorizontalAlignment(HorizontalAlignment.Center)
                       .CreatePart(player.AccountName, o => { 
                                  o.SetFontSize(ContentService.FontSize.Size16);

                                  if (player.KpProfile.NotFound) {
                                      o.SetPrefixImage(GameService.Content.GetTexture("common/1444522"));
                                      return;
                                  }

                                  o.SetLink(() => ProfileView.Open(player.KpProfile));

                              }).Build();

            accountName.BasicTooltipText = !player.KpProfile.NotFound ? player.KpProfile.ProofUrl : string.Empty;
            accountName.Parent           = this.View.Table;
            accountName.Visible          = false;

            var totals = player.KpProfile.Totals;

            var row = new List<object> {
                player.Icon, player.CharacterName, accountName
            };

            var tokens = ProofLogix.Instance.Resources.GetItemsForFractals()
                                   .Union(ProofLogix.Instance.Resources.GetGeneralItems())
                                   .Union(ProofLogix.Instance.Resources.GetItemsForMap(GameService.Gw2Mumble.CurrentMap.Id))
                                   .Select(i => totals.GetToken(i.Id)?.Amount).Cast<object>();
            
            row.AddRange(tokens);

            UpdateHeader();
            this.View.Table.ChangeData(key, row.ToArray());
        }

        public void UpdateHeader() {
            var row = new List<object> {
                string.Empty, "Character", "Account"
            };

            var tokens = ProofLogix.Instance.Resources.GetItemsForFractals()
                                   .Union(ProofLogix.Instance.Resources.GetGeneralItems())
                                   .Union(ProofLogix.Instance.Resources.GetItemsForMap(GameService.Gw2Mumble.CurrentMap.Id))
                                   .Select(item => item.Icon).Cast<object>();

            row.AddRange(tokens);

            this.View.Table.ChangeHeader(row.ToArray());
        }

        protected override void Unload() {
            ProofLogix.Instance.PartySync.PlayerAdded   -= PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerChanged -= PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerRemoved -= PlayerRemoved;
            base.Unload();
        }

        private void PlayerRemoved(object sender, ValueEventArgs<Player> e) {
            var key = e.Value.AccountName?.ToLowerInvariant();

            if (string.IsNullOrEmpty(key)) {
                return;
            }

            this.View.Table.RemoveData(key);
        }

        private void PlayerAddedOrChanged(object sender, ValueEventArgs<Player> e) {
            this.AddPlayer(e.Value);
        }
    }
}
