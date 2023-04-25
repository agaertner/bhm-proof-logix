using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Nekres.ProofLogix.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Nekres.ProofLogix.Core.Services.Resources;

namespace Nekres.ProofLogix.Core.UI.Table {
    public class TableView : View<TablePresenter> {

        public StandardTable<string> Table;

        public TableView(TableConfig config) {
            this.WithPresenter(new TablePresenter(this, config));
        }

        protected override void OnPresenterAssigned(TablePresenter presenter) {
            base.OnPresenterAssigned(presenter);
        }

        protected override Task<bool> Load(IProgress<string> progress) {
            return base.Load(progress);
        }

        protected override void Build(Container buildPanel) {
            Table = new StandardTable<string>(new object[] {
                string.Empty, "Character", "Account", "ID", 
                ResourceService.GetItemIcon((int)Item.LegendaryInsightGeneric),  
                ResourceService.GetItemIcon((int)Item.UnstableFractalEssence)
            }) {
                Parent = buildPanel,
                Width  = buildPanel.ContentRegion.Width,
                Height = buildPanel.ContentRegion.Height,
                Font = GameService.Content.DefaultFont16
            };

            foreach (var player in PartySyncService.PlayerList) {
                var key = player.AccountName?.ToLowerInvariant();

                if (!player.HasKpProfile || string.IsNullOrEmpty(key)) {
                    continue;
                }

                var totals = player.KpProfile.LinkedTotals;

                this.Table.ChangeData(key, new object[] {
                    player.Icon, player.CharacterName, player.AccountName, player.KpProfile?.Id ?? string.Empty,
                    totals.Killproofs.First(x => x.Id == (int)Item.LegendaryInsightGeneric).Amount, 
                    totals.Killproofs.First(x => x.Id == (int)Item.UnstableFractalEssence).Amount
                });
            }
            base.Build(buildPanel);
        }

        protected override void Unload() {
            base.Unload();
        }

    }
}
