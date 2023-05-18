using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Nekres.ProofLogix.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.UI.Table {
    public class TableView : View<TablePresenter> {

        public  StandardTable<string> Table;
        private Panel                 _panel;
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
            _panel = new Panel {
                Parent = buildPanel,
                Width  = buildPanel.ContentRegion.Width,
                Height = buildPanel.ContentRegion.Height,
                CanScroll = true
            };

            var row = new List<object> {
                string.Empty, "Character", "Account"
            };

            var tokens = ResourceService.GetItemIds().Select(ResourceService.GetItemIcon).Cast<object>();

            row.AddRange(tokens); 

            this.Table = new StandardTable<string>(row.ToArray()) {
                Parent = _panel,
                Width  = _panel.Width,
                Height = _panel.Height,
                Font   = GameService.Content.DefaultFont16
            };

            buildPanel.ContentResized += OnResized;

            foreach (var player in PartySyncService.PlayerList) {
                this.Presenter.AddPlayer(player);
            }
            base.Build(buildPanel);
        }

        private void OnResized(object sender, RegionChangedEventArgs e) {
            _panel.Size = e.CurrentRegion.Size;
        }

        protected override void Unload() {
            base.Unload();
        }
    }
}
