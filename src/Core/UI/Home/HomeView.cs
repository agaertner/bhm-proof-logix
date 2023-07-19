using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Nekres.ProofLogix.Core.UI.Clears;
using Nekres.ProofLogix.Core.UI.Table;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.ProofLogix.Core.UI.Home {
    public class HomeView : View<HomePresenter> {

        public FlowPanel HistoryPanel;

        public HomeView() {
            this.WithPresenter(new HomePresenter(this, null));
        }

        protected override void Build(Container buildPanel) {

            var plyPanel = new Panel {
                Parent           = buildPanel,
                Width            = buildPanel.ContentRegion.Width  - buildPanel.ContentRegion.Width  / 3,
                Height           = buildPanel.ContentRegion.Height - buildPanel.ContentRegion.Height / 3,
                ShowBorder       = true,
                Title            = "Game Account",
                BasicTooltipText = "Shows the current snapshot of your account data.\nEnables you to verify if newly acquired progress is\nalready available for tracking by third-parties."
            };

            var plyPanelBody = new ViewContainer {
                Parent = plyPanel,
                Width  = plyPanel.ContentRegion.Width,
                Height = plyPanel.ContentRegion.Height - 32 - Panel.BOTTOM_PADDING
            };

            var proofsBttn = new StandardButton {
                Parent = plyPanel,
                Width  = plyPanel.ContentRegion.Width / 3,
                Height = 32,
                Top    = plyPanelBody.Bottom + Panel.BOTTOM_PADDING,
                Left = Panel.RIGHT_PADDING,
                Text   = "Proofs"
            };

            var clearsBttn = new StandardButton {
                Parent = plyPanel,
                Width  = plyPanel.ContentRegion.Width / 3,
                Height = 32,
                Top    = proofsBttn.Top,
                Left   = proofsBttn.Right + Panel.RIGHT_PADDING,
                Text   = "Clears"
            };

            plyPanel.ContentResized += (_, e) => {
                plyPanelBody.Width  = e.CurrentRegion.Width;
                plyPanelBody.Height = e.CurrentRegion.Height - 32 - Panel.BOTTOM_PADDING;
                proofsBttn.Width    = e.CurrentRegion.Width / 3;
                proofsBttn.Top      = plyPanelBody.Bottom + Panel.BOTTOM_PADDING;
                proofsBttn.Left     = Panel.RIGHT_PADDING;
                proofsBttn.Bottom   = e.CurrentRegion.Height - Panel.BOTTOM_PADDING;
                clearsBttn.Width    = e.CurrentRegion.Width / 3;
                clearsBttn.Top      = proofsBttn.Top;
                clearsBttn.Left     = proofsBttn.Right + Panel.RIGHT_PADDING;
                clearsBttn.Bottom   = proofsBttn.Bottom;
            };

            var navPanel = new Panel {
                Parent     = buildPanel,
                Left       = plyPanel.Right,
                Width      = buildPanel.ContentRegion.Width / 3,
                Height     = buildPanel.ContentRegion.Height - buildPanel.ContentRegion.Width / 3,
                ShowBorder = true,
                Title = "Menu"
            };

            var arcDpsBridge = "ArcDPS Bridge";
            string prefixImage;
            string tooltipText;

            if (GameService.ArcDps.Running) {
                prefixImage = "common/157330";
                tooltipText = string.Empty;
            } else {
                prefixImage = "common/1444522";
                tooltipText = "ArcDPS Bridge not running.";
            }

            var textSize     = LabelUtil.GetLabelSize(ContentService.FontSize.Size11, arcDpsBridge, true);
            var textLabel = new FormattedLabelBuilder().SetWidth(textSize.X).SetHeight(textSize.Y).CreatePart(arcDpsBridge, o => {
                o.SetFontSize(ContentService.FontSize.Size11);
                o.SetPrefixImage(GameService.Content.GetTexture(prefixImage));
            }).Build();
            textLabel.Parent           = navPanel;
            textLabel.Bottom           = textLabel.Parent.ContentRegion.Height - Control.ControlStandard.ControlOffset.Y;
            textLabel.Right            = textLabel.Parent.ContentRegion.Width  - Control.ControlStandard.ControlOffset.X;
            textLabel.BasicTooltipText = tooltipText;

            var openTableBttn = new StandardButton() {
                Parent = navPanel,
                Width  = 112,
                Height = 32,
                Left   = (navPanel.ContentRegion.Width - 112) / 2,
                Top    = Panel.TOP_PADDING,
                Text   = "Party Table"
            };

            openTableBttn.Click += (_,_) => ProofLogix.Instance.ToggleTable();

            var openMyProfileBttn = new StandardButton() {
                Parent = navPanel,
                Width  = 112,
                Height = 32,
                Left   = openTableBttn.Left,
                Top    = openTableBttn.Bottom + Control.ControlStandard.ControlOffset.Y,
                Text   = "My Profile"
            };

            openMyProfileBttn.Click += (_, _) => {
                if (!ProofLogix.Instance.PartySync.LocalPlayer.HasKpProfile) {

                    return;
                }
                ProfileView.Open(ProofLogix.Instance.PartySync.LocalPlayer.KpProfile);
            };

            this.HistoryPanel = new FlowPanel {
                Parent              = buildPanel,
                Top                 = plyPanel.Bottom,
                Width               = buildPanel.ContentRegion.Width,
                Height              = buildPanel.ContentRegion.Height / 3,
                ShowBorder          = true,
                ControlPadding      = new Vector2(Control.ControlStandard.ControlOffset.X, Control.ControlStandard.ControlOffset.Y),
                OuterControlPadding = new Vector2(Panel.LEFT_PADDING,                      Panel.TOP_PADDING),
                Padding             = new Thickness(5, 5),
                FlowDirection       = ControlFlowDirection.SingleTopToBottom,
                CanScroll           = true,
                Title               = "History"
            };

            foreach (var player in ProofLogix.Instance.PartySync.HistoryList) {
                this.Presenter.AddHistoryEntry(player);
            }

            buildPanel.ContentResized += (_, e) => {
                plyPanel.Width  = e.CurrentRegion.Width  - e.CurrentRegion.Width  / 3;
                plyPanel.Height = e.CurrentRegion.Height - e.CurrentRegion.Height / 3;
                navPanel.Width  = e.CurrentRegion.Width / 3;
                navPanel.Height = e.CurrentRegion.Height - e.CurrentRegion.Height / 3;

                navPanel.Left            = plyPanel.Right;

                if (this.HistoryPanel != null) {
                    this.HistoryPanel.Width  = e.CurrentRegion.Width;
                    this.HistoryPanel.Height = e.CurrentRegion.Height / 3;
                    this.HistoryPanel.Top    = plyPanel.Bottom;
                }

                textLabel.Bottom       = textLabel.Parent.ContentRegion.Height - Control.ControlStandard.ControlOffset.Y;
                textLabel.Right        = textLabel.Parent.ContentRegion.Width  - Control.ControlStandard.ControlOffset.X;
                openTableBttn.Left     = (openTableBttn.Parent.ContentRegion.Width - openTableBttn.Width) / 2;
                openMyProfileBttn.Left = openTableBttn.Left;
                openMyProfileBttn.Top  = openTableBttn.Bottom + Control.ControlStandard.ControlOffset.Y;
            };

            proofsBttn.Click += async (_, _) => {
                if (!ProofLogix.Instance.Gw2WebApi.HasPermissions) {
                    ScreenNotification.ShowNotification("Insufficient permissions.", ScreenNotification.NotificationType.Error);
                    return;
                }

                plyPanelBody.Show(new LoadingView("Loading items.."));

                var bank             = await ProofLogix.Instance.Gw2WebApi.GetBank();
                var sharedBags       = await ProofLogix.Instance.Gw2WebApi.GetSharedBags();
                var bagsByCharacters = await ProofLogix.Instance.Gw2WebApi.GetBagsByCharacter();
                plyPanelBody.Show(new AccountItemsView(bank, sharedBags, bagsByCharacters));
            };

            clearsBttn.Click += async (_, _) => {
                if (!ProofLogix.Instance.Gw2WebApi.HasPermissions) {
                    ScreenNotification.ShowNotification("Insufficient permissions.", ScreenNotification.NotificationType.Error);
                    return;
                }

                plyPanelBody.Show(new LoadingView("Loading clears.."));
                plyPanelBody.Show(new ClearsView(await ProofLogix.Instance.Gw2WebApi.GetClears()));
            };

            base.Build(buildPanel);
        }

        private class AccountItemsView : View {

            private readonly List<AccountItem> _bank;
            private readonly List<AccountItem> _sharedBags;
            private readonly Dictionary<string, List<AccountItem>> _bags;

            public AccountItemsView(List<AccountItem> bank,
                                    List<AccountItem> sharedBags,
                                    Dictionary<string, List<AccountItem>> bags) {
                _bank = bank;
                _sharedBags = sharedBags;
                _bags = bags;
            }

            protected override void Build(Container buildPanel) {

                var itemsPanel = new FlowPanel {
                    Parent              = buildPanel,
                    Width               = buildPanel.ContentRegion.Width,
                    Height              = buildPanel.ContentRegion.Height,
                    CanScroll           = true,
                    OuterControlPadding = new Vector2(Panel.LEFT_PADDING, Panel.TOP_PADDING)
                };

                buildPanel.ContentResized += (_, e) => {
                    itemsPanel.Width = e.CurrentRegion.Width;
                    itemsPanel.Height = e.CurrentRegion.Height;
                };

                AddItems(itemsPanel, _bank, "Bank");
                AddItems(itemsPanel, _sharedBags, "Shared Bags");

                foreach (var bagsByChar in _bags) {
                    AddItems(itemsPanel, bagsByChar.Value, bagsByChar.Key);
                }

                base.Build(buildPanel);
            }

            private void AddItems(FlowPanel parent, List<AccountItem> items, string category) {
                if (!items.Any()) {
                    return;
                }

                var slotsCategory = new FlowPanel {
                    Parent = parent,
                    Width = parent.ContentRegion.Width - 24,
                    HeightSizingMode = SizingMode.AutoSize,
                    Title = category,
                    CanCollapse = true,
                    CanScroll = true,
                    OuterControlPadding = new Vector2(5, 5),
                    ControlPadding = new Vector2(5, 5)
                };

                parent.ContentResized += (_, e) => {
                    slotsCategory.Width = e.CurrentRegion.Width - 24;
                };

                foreach (var item in items) {

                    var name = ProofLogix.Instance.Resources.GetItems().FirstOrDefault(i => i.Id.Equals(item.Id))?.Name;

                    if (!string.IsNullOrEmpty(name)) {
                        name = $"{item.Count}x {name}";
                    }

                    var slotItem = new ItemWithAmount(ProofLogix.Instance.Resources.GetResource(item.Id).Icon) {
                        Parent = slotsCategory,
                        Width = 64,
                        Height = 64,
                        Amount = item.Count,
                        BasicTooltipText = name
                    };
                }
            }

        }
    }
}
