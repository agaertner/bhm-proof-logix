using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Nekres.ProofLogix.Core.UI.Clears;
using Nekres.ProofLogix.Core.UI.KpProfile;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.ProofLogix.Core.UI.Home {
    public class HomeView : View {

        //history icon:733360
        private AsyncTexture2D _kpIcon;

        public HomeView() {
            _kpIcon = ProofLogix.Instance.ContentsManager.GetTexture("killproof_icon.png");
        }

        protected override void Unload() {
            _kpIcon.Dispose();
            base.Unload();
        }

        protected override void Build(Container buildPanel) {

            var menuPanel = new Panel {
                Parent    = buildPanel,
                Width     = 200,
                Height    = buildPanel.ContentRegion.Height,
                CanScroll = true,
                Title     = "Game Account"
            };

            var navMenu = new Menu {
                Parent = menuPanel,
                Top    = 0,
                Left   = 0,
                Width  = menuPanel.ContentRegion.Width,
                Height = menuPanel.ContentRegion.Height,
            };

            var proofsEntry = new MenuItem {
                Parent = navMenu,
                Text   = "Owned Proofs",
                Width  = navMenu.ContentRegion.Width,
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156699)
            };

            var clearsEntry = new MenuItem {
                Parent = navMenu,
                Text   = "Weekly Clears",
                Width  = navMenu.ContentRegion.Width,
                Icon   = GameService.Content.DatAssetCache.GetTextureFromAssetId(1234912)
            };

            var separatorEntry = new MenuItem {
                Parent = navMenu,
                Text   = "Other",
                Width  = navMenu.ContentRegion.Width,
                Collapsed = false
            };

            var myProfileEntry = new MenuItem {
                Parent = separatorEntry,
                Text   = "My Profile",
                Width  = navMenu.ContentRegion.Width,
                Icon = _kpIcon,
            };

            var squadTableEntry = new MenuItem {
                Parent = separatorEntry,
                Text   = "Party Table",
                Width  = navMenu.ContentRegion.Width,
                Icon   = GameService.Content.DatAssetCache.GetTextureFromAssetId(156407)
            };

            var plyPanel = new ViewContainer {
                Parent           = buildPanel,
                Left             = menuPanel.Right,
                Width            = buildPanel.ContentRegion.Width - menuPanel.Width,
                Height           = buildPanel.ContentRegion.Height,
                ShowBorder       = true,
                BasicTooltipText = "Shows the current snapshot of your account data.\nEnables you to verify if newly acquired progress is\nalready available for tracking by third-parties."
            };

            buildPanel.ContentResized += (_, e) => {
                plyPanel.Width  = e.CurrentRegion.Width - menuPanel.Width;
                plyPanel.Height = e.CurrentRegion.Height;
            };

            menuPanel.ContentResized += (_, e) => {
                navMenu.Width  = e.CurrentRegion.Width;
                navMenu.Height = e.CurrentRegion.Height;
            };

            navMenu.ContentResized += (_, e) => {
                proofsEntry.Width      = e.CurrentRegion.Width;
                clearsEntry.Height     = e.CurrentRegion.Height;
                separatorEntry.Width   = e.CurrentRegion.Width;
                myProfileEntry.Height  = e.CurrentRegion.Height;
                squadTableEntry.Height = e.CurrentRegion.Height;
            };

            proofsEntry.Click += async (_, _) => {
                if (!ProofLogix.Instance.Gw2WebApi.HasPermissions) {
                    ScreenNotification.ShowNotification("Insufficient permissions.", ScreenNotification.NotificationType.Error);
                    return;
                }

                plyPanel.Show(new LoadingView("Loading items.."));

                var bank             = await ProofLogix.Instance.Gw2WebApi.GetBank();
                var sharedBags       = await ProofLogix.Instance.Gw2WebApi.GetSharedBags();
                var bagsByCharacters = await ProofLogix.Instance.Gw2WebApi.GetBagsByCharacter();
                plyPanel.Show(new AccountItemsView(bank, sharedBags, bagsByCharacters));
            };

            clearsEntry.Click += async (_, _) => {
                if (!ProofLogix.Instance.Gw2WebApi.HasPermissions) {
                    ScreenNotification.ShowNotification("Insufficient permissions.", ScreenNotification.NotificationType.Error);
                    return;
                }

                plyPanel.Show(new LoadingView("Loading clears.."));
                plyPanel.Show(new ClearsView(await ProofLogix.Instance.Gw2WebApi.GetClears()));
            };

            myProfileEntry.Click += (_, _) => {
                if (!ProofLogix.Instance.PartySync.LocalPlayer.HasKpProfile) {
                    return;
                }

                ProfileView.Open(ProofLogix.Instance.PartySync.LocalPlayer.KpProfile);
            };

            squadTableEntry.Click += (_, _) => ProofLogix.Instance.ToggleTable();

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
