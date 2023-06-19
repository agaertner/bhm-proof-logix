using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Nekres.ProofLogix.Core.Services;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using System;

namespace Nekres.ProofLogix.Core.UI.LookingForOpener {
    public class LfoView : View<LfoPresenter>{

        public LfoView(LfoConfig model) {
            this.WithPresenter(new LfoPresenter(this, model));
        }

        protected override void Build(Container buildPanel) {

            var header = new Panel {
                Parent = buildPanel,
                Width = buildPanel.ContentRegion.Width,
                Height = 20
            };

            var regionSelect = new Dropdown {
                Parent = header,
                Left  = header.ContentRegion.Width - 50,
                Width  = 50,
                Height = header.ContentRegion.Height
            };
            foreach (var region in Enum.GetNames(typeof(Opener.ServerRegion))) {
                regionSelect.Items.Add(region);
            }
            regionSelect.SelectedItem =  ProofLogix.Instance.Region.Value;
            regionSelect.ValueChanged += (_, e) => this.Presenter.SetRegion(e.CurrentValue);

            header.ContentResized += (_, e) => {
                regionSelect.Left = e.CurrentRegion.Width - regionSelect.Width;
            };

            var menuPanel = new Panel {
                Parent = buildPanel,
                Width  = 200,
                Height = buildPanel.ContentRegion.Height - header.Height,
                Top = header.Bottom,
                CanScroll = true,
                Title  = "Select an Encounter"
            };

            var resultContainer = new ViewContainer {
                Parent     = buildPanel,
                ShowBorder = true,
                Left       = menuPanel.Width + Panel.MenuStandard.PanelOffset.X,
                Width      = buildPanel.ContentRegion.Width - menuPanel.Width,
                Height     = buildPanel.ContentRegion.Height - header.Height,
                Top = header.Bottom
            };

            buildPanel.ContentResized += (_, e) => {
                menuPanel.Height       = e.CurrentRegion.Height - header.Height;
                resultContainer.Height = e.CurrentRegion.Height - header.Height;
                resultContainer.Width  = e.CurrentRegion.Width  - menuPanel.Width;
                header.Width           = e.CurrentRegion.Width;
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
