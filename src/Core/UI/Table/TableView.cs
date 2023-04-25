using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Nekres.ProofLogix.Core.Services;
using Nekres.ProofLogix.Core.Services.Resources;
using System;
using System.Threading.Tasks;

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
                string.Empty, "Character", "Account", 
                ResourceService.GetItemIcon((int)Item.LegendaryInsightGeneric),  
                ResourceService.GetItemIcon((int)Item.UnstableFractalEssence)
            }) {
                Parent = buildPanel,
                Width  = buildPanel.ContentRegion.Width,
                Height = buildPanel.ContentRegion.Height,
                Font = GameService.Content.DefaultFont16
            };

            buildPanel.ContentResized += OnResized;

            foreach (var player in PartySyncService.PlayerList) {
                this.Presenter.AddPlayer(player);
            }
            base.Build(buildPanel);
        }

        private void OnResized(object sender, RegionChangedEventArgs e) {
            this.Table.Size = e.CurrentRegion.Size;
        }

        protected override void Unload() {
            base.Unload();
        }

    }
}
