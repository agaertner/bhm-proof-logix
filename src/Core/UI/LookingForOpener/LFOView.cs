using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Nekres.ProofLogix.Core.Services;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using System;
using Blish_HUD;

namespace Nekres.ProofLogix.Core.UI.LookingForOpener {
    public class LfoView : View<LfoPresenter>{

        public LfoView(LfoConfig model) {
            this.WithPresenter(new LfoPresenter(this, model));
        }

        protected override void Build(Container buildPanel) {

            var footer = new FlowPanel {
                Parent = buildPanel,
                Width  = buildPanel.ContentRegion.Width,
                Bottom = buildPanel.ContentRegion.Height,
                FlowDirection = ControlFlowDirection.LeftToRight,
                Height = 20
            };

            var menuPanel = new Panel {
                Parent    = buildPanel,
                Width     = 200,
                Height    = buildPanel.ContentRegion.Height - footer.Height,
                CanScroll = true,
                Title     = "Select an Encounter"
            };

            var resultContainer = new ViewContainer {
                Parent     = buildPanel,
                ShowBorder = true,
                Left       = menuPanel.Width + Panel.LEFT_PADDING,
                Width      = buildPanel.ContentRegion.Width - menuPanel.Width,
                Height     = buildPanel.ContentRegion.Height - footer.Height
            };

            var regionLabel = new Label {
                Parent     = footer,
                Width      = 50,
                StrokeText = true,
                Text       = "Region:"
            };

            var regionSelect = new Dropdown {
                Parent = footer,
                Width  = 50,
                Height = footer.ContentRegion.Height
            };

            foreach (var region in Enum.GetNames(typeof(Opener.ServerRegion))) {
                regionSelect.Items.Add(region);
            }
            regionSelect.SelectedItem =  ProofLogix.Instance.Region.Value;
            regionSelect.ValueChanged += (_, e) => this.Presenter.SetRegion(e.CurrentValue);

            buildPanel.ContentResized += (_, e) => {
                menuPanel.Height       = e.CurrentRegion.Height - footer.Height;
                resultContainer.Height = e.CurrentRegion.Height - footer.Height;
                resultContainer.Width  = e.CurrentRegion.Width  - menuPanel.Width;
                footer.Width           = e.CurrentRegion.Width;
            };

            var menu = new Menu {
                Parent = menuPanel,
                Width  = menuPanel.ContentRegion.Width,
                Height = menuPanel.ContentRegion.Height
            };

            menuPanel.ContentResized += (_, e) => {
                menu.Height = e.CurrentRegion.Height;
                menu.Width  = e.CurrentRegion.Width;
            };

            var wings = ProofLogix.Instance.Resources.GetWings();

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
