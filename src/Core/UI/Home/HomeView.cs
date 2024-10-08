﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Extended;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Nekres.ProofLogix.Core.UI.Clears;
using Nekres.ProofLogix.Core.UI.KpProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Extended.Properties;

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
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156699),
                BasicTooltipText = "Shows the current snapshot of your inventories.\nEnables you to verify if recent rewarded proofs are\nalready available to be recorded by killproof.me."
            };

            var clearsEntry = new MenuItem {
                Parent           = navMenu,
                Text             = "Weekly Clears",
                Width            = navMenu.ContentRegion.Width,
                Icon             = GameService.Content.DatAssetCache.GetTextureFromAssetId(1234912),
                BasicTooltipText = "Shows the current snapshot of your clears.\nEnables you to verify if recent completed encounters are\nalready available to be recorded by killproof.me."
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

            var smartPingEntry = new MenuItem {
                Parent = separatorEntry,
                Text   = "Smart Ping",
                Width  = navMenu.ContentRegion.Width,
                Icon   = GameService.Content.DatAssetCache.GetTextureFromAssetId(155157),
            };

            var plyPanel = new ViewContainer {
                Parent           = buildPanel,
                Left             = menuPanel.Right,
                Width            = buildPanel.ContentRegion.Width - menuPanel.Width,
                Height           = buildPanel.ContentRegion.Height,
                ShowBorder       = true
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
                smartPingEntry.Height  = e.CurrentRegion.Height;
            };

            proofsEntry.Click += async (_, _) => {

                if (!ProofLogix.Instance.Resources.HasLoaded()) {
                    GameService.Content.PlaySoundEffectByName("error");
                    return;
                }

                if (!ProofLogix.Instance.Gw2ApiManager.IsAuthorized(true, ProofLogix.Instance.Gw2WebApi.Requires)) {
                    GameService.Content.PlaySoundEffectByName("error");
                    return;
                }

                var loadingText = new AsyncString();
                plyPanel.Show(new LoadingView("Loading items…", loadingText));

                loadingText.String = ProofLogix.Instance.Resources.GetLoadingSubtitle();
                var bank = await ProofLogix.Instance.Gw2WebApi.GetBank();

                loadingText.String = ProofLogix.Instance.Resources.GetLoadingSubtitle();
                var sharedBags = await ProofLogix.Instance.Gw2WebApi.GetSharedBags();

                loadingText.String = ProofLogix.Instance.Resources.GetLoadingSubtitle();
                var bagsByCharacters = await ProofLogix.Instance.Gw2WebApi.GetBagsByCharacter();

                plyPanel.Show(new AccountItemsView(bank, sharedBags, bagsByCharacters));
            };

            clearsEntry.Click += async (_, _) => {

                if (!ProofLogix.Instance.Resources.HasLoaded()) {
                    GameService.Content.PlaySoundEffectByName("error");
                    return;
                }

                if (!ProofLogix.Instance.Gw2ApiManager.IsAuthorized(true, ProofLogix.Instance.Gw2WebApi.Requires)) {
                    GameService.Content.PlaySoundEffectByName("error");
                    return;
                }

                plyPanel.Show(new LoadingView("Loading clears.."));
                plyPanel.Show(new ClearsView(await ProofLogix.Instance.Gw2WebApi.GetClears()));
            };

            myProfileEntry.Click += (_, _) => {

                if (!ProofLogix.Instance.Resources.HasLoaded()) {
                    GameService.Content.PlaySoundEffectByName("error");
                    return;
                }

                var localPlayer = ProofLogix.Instance.PartySync.LocalPlayer;

                if (!localPlayer.HasKpProfile) {

                    GameService.Content.PlaySoundEffectByName("error");

                    if (string.IsNullOrWhiteSpace(GameService.Gw2Mumble.PlayerCharacter.Name)) {
                        ScreenNotification.ShowNotification("Profile unavailable. Please, login to a character.", ScreenNotification.NotificationType.Error);
                        return;
                    }

                    ScreenNotification.ShowNotification("Not yet loaded. Please, try again.", ScreenNotification.NotificationType.Error);
                    return;
                }

                if (localPlayer.KpProfile.NotFound) {
                    ProofLogix.Instance.ToggleRegisterWindow();
                    return;
                }

                ProfileView.Open(localPlayer.KpProfile);
            };

            squadTableEntry.Click += (_, _) => {
                ProofLogix.Instance.ToggleTable();
            };

            smartPingEntry.Click += (_, _) => {
                ProofLogix.Instance.ToggleSmartPing();
            };

            base.Build(buildPanel);
        }

        private class AccountItemsView : View {

            private readonly List<AccountItem>                        _bank;
            private readonly List<AccountItem>                        _sharedBags;
            private readonly Dictionary<Character, List<AccountItem>> _bags;

            public AccountItemsView(List<AccountItem> bank,
                                    List<AccountItem> sharedBags,
                                    Dictionary<Character, List<AccountItem>> bags) {
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

                AddItems(itemsPanel, _bank,       "Account Vault",          GameService.Content.DatAssetCache.GetTextureFromAssetId(156699));
                AddItems(itemsPanel, _sharedBags, "Shared Inventory Slots", GameService.Content.DatAssetCache.GetTextureFromAssetId(1314214));

                foreach (var bagsByChar in _bags) {

                    var elite = bagsByChar.Key.BuildTabs?.First(tab => tab.IsActive).Build.Specializations[2].Id ?? 0;

                    if (!Enum.TryParse<ProfessionType>(bagsByChar.Key.Profession, true, out var profession)) {
                        ProofLogix.Logger.Warn("Unable to cast '{0}' to {1}.", bagsByChar.Key.Profession, nameof(ProfessionType));
                        continue;
                    }

                    AddItems(itemsPanel, bagsByChar.Value, bagsByChar.Key.Name, 
                             ProofLogix.Instance.Resources.GetClassIcon((int)profession, elite));
                }

                base.Build(buildPanel);
            }

            private void AddItems(FlowPanel parent, List<AccountItem> items, string category, AsyncTexture2D icon) {
                if (!items.Any()) {
                    return;
                }

                var slotsCategory = new FlowPanelWithIcon(icon) {
                    Parent              = parent,
                    Width               = parent.ContentRegion.Width - 24,
                    HeightSizingMode    = SizingMode.AutoSize,
                    Title               = category,
                    CanCollapse         = true,
                    CanScroll           = true,
                    OuterControlPadding = new Vector2(5, 5),
                    ControlPadding      = new Vector2(5, 5)
                };

                parent.ContentResized += (_, e) => {
                    slotsCategory.Width = e.CurrentRegion.Width - 24;
                };

                foreach (var item in items) {
                    ItemWithAmount.Create(item.Id, item.Count).Parent = slotsCategory;
                }
            }
        }
    }
}
