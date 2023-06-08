using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Nekres.ProofLogix.Core.Services;

namespace Nekres.ProofLogix.Core.UI.LookingForOpener {
    public class LfoView : View<LfoPresenter>{

        public LfoView(LfoConfig model) {
            this.WithPresenter(new LfoPresenter(this, model));
        }

        protected override void Build(Container buildPanel) {

            var menuPanel = new Panel {
                Parent = buildPanel,
                Width  = 200,
                Height = buildPanel.ContentRegion.Height,
                CanScroll = true,
                Title  = "Select an Encounter"
            };

            var resultContainer = new ViewContainer {
                Parent     = buildPanel,
                ShowBorder = true,
                Left       = menuPanel.Width + Panel.MenuStandard.PanelOffset.X,
                Width      = buildPanel.ContentRegion.Width - 200,
                Height     = buildPanel.ContentRegion.Height
            };

            buildPanel.ContentResized += (_, e) => {
                menuPanel.Height = e.CurrentRegion.Height;
                resultContainer.Height = e.CurrentRegion.Height;
            };

            var menu = new Menu {
                Parent = menuPanel,
                Top = 0,
                Left = 0,
                Width = menuPanel.ContentRegion.Width,
                Height = menuPanel.ContentRegion.Height
            };

            menuPanel.ContentResized += (_, e) => {
                menu.Height = e.CurrentRegion.Height;
            };

            var wings = ResourceService.GetWings();

            int wingNr = 0;
            foreach (var wing in wings) {
                wingNr++;


                var wingItem = new MenuItem {
                    Parent = menu,
                    Text = $"Wing {wingNr}"
                };

                foreach (var encounter in wing.Events) {

                    var encounterItem = new MenuItem {
                        Parent = wingItem,
                        Text   = encounter.Name,
                        Icon   = encounter.Icon,
                        Width  = menu.ContentRegion.Width
                    };

                    encounterItem.Click += async (_, _) => {
                        this.Presenter.SetEncounterId(encounter.Id);
                        resultContainer.Show(new LoadingView("Searching..."));
                        resultContainer
                           .Show(new LfoResultView(new LfoResults(this.Presenter.Model.EncounterId, await this.Presenter.GetOpener())));
                    };
                }
            }

            base.Build(buildPanel);
        }
    }
}
