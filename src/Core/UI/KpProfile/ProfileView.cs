﻿using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Extended;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using Nekres.ProofLogix.Core.UI.Clears;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models.Title;
using Container = Blish_HUD.Controls.Container;
using Profile = Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models.Profile;
using Title = Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models.Title;
using Token = Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models.Token;
namespace Nekres.ProofLogix.Core.UI.KpProfile {

    public class LinkedView : View {

        private readonly Profile _profile;

        public LinkedView(Profile profile) {
            _profile = profile;
        }

        protected override void Build(Container buildPanel) {

            var profileContainer = new ViewContainer {
                Parent = buildPanel,
                Width  = buildPanel.ContentRegion.Width,
                Height = buildPanel.ContentRegion.Height
            };

            if (_profile.Linked == null || !_profile.Linked.Any()) {

                buildPanel.ContentResized += (_, e) => {
                    profileContainer.Width  = e.CurrentRegion.Width;
                    profileContainer.Height = e.CurrentRegion.Height;
                };

                profileContainer.Show(new ProfileView(_profile));

                base.Build(buildPanel);
                return;
            }

            var menuPanel = new Panel {
                Parent    = buildPanel,
                Width     = 100,
                Height    = buildPanel.ContentRegion.Height,
                CanScroll = true,
                Title     = "Accounts"
            };

            var menu = new Menu {
                Parent = menuPanel,
                Width  = menuPanel.ContentRegion.Width,
                Height = menuPanel.ContentRegion.Height
            };

            menuPanel.ContentResized += (_, e) => {
                menu.Height = e.CurrentRegion.Height;
            };

            profileContainer.Width   -= menuPanel.Width;
            profileContainer.Left    =  menuPanel.Right;

            foreach (var profile in _profile.Accounts) {
                var entry = new MenuItem(AssetUtil.Truncate(profile.Name, menu.ContentRegion.Width - 14, GameService.Content.DefaultFont16)) {
                    Parent = menu,
                    BasicTooltipText = profile.Name,
                    Width = menu.ContentRegion.Width
                };

                entry.ItemSelected += (_,_) => {
                    profileContainer.Show(new ProfileView(profile));
                };

            }

            buildPanel.ContentResized += (_, e) => {
                profileContainer.Width  = e.CurrentRegion.Width - menu.Width;
                profileContainer.Height = e.CurrentRegion.Height;
                profileContainer.Left   = menuPanel.Right;
                menuPanel.Height        = buildPanel.ContentRegion.Height;
            };

            profileContainer.Show(new ProfileView(_profile));

            base.Build(buildPanel);
        }
    }

    public class ProfileView : View {

        private readonly Profile _profile;

        public ProfileView(Profile profile) {
            _profile = profile;
        }

        public static void Open(Profile profile) {
            if (string.IsNullOrEmpty(profile.Name)) {
                return;
            }

            foreach (string name in profile.Accounts.Select(link => link.Name)) {
                if (!TrackableWindow.TryGetById(name.ToLowerInvariant(), out var wnd)) {
                    continue;
                }

                wnd.Left = (GameService.Graphics.SpriteScreen.Width  - wnd.Width)  / 2;
                wnd.Top  = (GameService.Graphics.SpriteScreen.Height - wnd.Height) / 2;
                wnd.BringWindowToFront();
                wnd.Show(new LinkedView(profile));
                return;
            }

            var window = new TrackableWindow(profile.Name.ToLowerInvariant(), GameService.Content.DatAssetCache.GetTextureFromAssetId(155985),
                                             new Rectangle(40, 26, 913, 691),
                                             new Rectangle(70, 36, 839, 605)) {
                Parent    = GameService.Graphics.SpriteScreen,
                Title     = "Profile",
                Subtitle  = profile.Id,
                Id        = $"{nameof(ProofLogix)}_Profile_a32c972dd9fe4025a01d3256025ab1dc",
                CanResize = true,
                SavesSize = true,
                Width     = 800,
                Height    = 600,
                Left      = (GameService.Graphics.SpriteScreen.Width  - 700) / 2,
                Top       = (GameService.Graphics.SpriteScreen.Height - 600) / 2,
                Emblem    = ProofLogix.Instance.Emblem
            };

            window.Show(new LinkedView(profile));
        }

        protected override void Build(Container buildPanel) {
            var header = new FlowPanel {
                Parent              = buildPanel,
                Width               = buildPanel.ContentRegion.Width,
                Height              = 100,
                ControlPadding      = new Vector2(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                FlowDirection       = ControlFlowDirection.SingleLeftToRight,
                ShowBorder          = true
            };

            var info = new FlowPanel {
                Parent              = header,
                Width               = (int)(0.4f * header.ContentRegion.Width),
                Height              = header.ContentRegion.Height,
                ControlPadding      = new Vector2(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                FlowDirection       = ControlFlowDirection.SingleTopToBottom
            };

            var navMenu = new FlowPanel {
                Parent              = header,
                Width               = (int)(0.6f * header.ContentRegion.Width),
                Height              = header.ContentRegion.Height,
                Right               = header.ContentRegion.Width,
                ControlPadding      = new Vector2(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                FlowDirection       = ControlFlowDirection.SingleRightToLeft,
            };

            var nameSize = LabelUtil.GetLabelSize(ContentService.FontSize.Size18, _profile.Name);
            var name = new FormattedLabelBuilder().SetWidth(nameSize.X).SetHeight(nameSize.Y)
                                                  .CreatePart(_profile.Name, o => {
                                                       o.SetFontSize(ContentService.FontSize.Size18);
                                                       o.SetLink(() => {
                                                           GameService.Content.PlaySoundEffectByName("button-click");
                                                           Process.Start(_profile.ProofUrl);
                                                       });
                                                   }).Build();
            name.Parent = info;

            var lastRefreshText = _profile.LastRefresh.ToLocalTime().AsTimeAgo();
            var size            = LabelUtil.GetLabelSize(ContentService.FontSize.Size11, lastRefreshText);
            var lastRefresh     = new FormattedLabelBuilder().SetWidth(size.X).SetHeight(size.Y)
                                                             .CreatePart(lastRefreshText, o => {
                                                                  o.SetFontSize(ContentService.FontSize.Size11);
                                                                  o.MakeItalic();
                                                              }).Build();
            lastRefresh.Parent = info;

            header.ContentResized += (_, e) => {
                info.Width     = (int)(0.4f * header.ContentRegion.Width);
                info.Height    = e.CurrentRegion.Height;
                navMenu.Width  = (int)(0.6f * header.ContentRegion.Width);
                navMenu.Height = e.CurrentRegion.Height;
                navMenu.Right  = e.CurrentRegion.Width;
            };

            var body = new ViewContainer {
                Parent     = buildPanel,
                Top        = header.Bottom + Panel.TOP_PADDING,
                Width      = buildPanel.ContentRegion.Width,
                Height     = buildPanel.ContentRegion.Height - header.Height - Panel.TOP_PADDING,
                ShowBorder = true
            };

            buildPanel.ContentResized += (_, e) => {
                header.Width = e.CurrentRegion.Width;
                body.Width  = e.CurrentRegion.Width;
                body.Height = e.CurrentRegion.Height - header.Height - Panel.TOP_PADDING;
            };

            body.Show(new ItemsView(_profile));

            var b1 = new StandardButton {
                Parent = navMenu,
                Width  = 150,
                Height = 30,
                Text   = "Weekly Clears"
            };

            b1.Click += (_, _) => {
                GameService.Content.PlaySoundEffectByName("button-click");
                body.Show(new ClearsView(_profile.Clears));
            };

            var b2 = new StandardButton {
                Parent = navMenu,
                Width  = 150,
                Height = 30,
                Text   = "Proofs"
            };

            b2.Click += (_, _) => {
                GameService.Content.PlaySoundEffectByName("button-click");
                body.Show(new ItemsView(_profile));
            };

            var refreshBttn = new RefreshButton {
                Parent = navMenu,
                Width = 32,
                Height = 32,
                NextRefresh = _profile.NextRefresh
            };

            var isRefreshing = false;
            refreshBttn.Click += async (_, _) => {
                if (isRefreshing || _profile.NextRefresh > DateTime.UtcNow) {
                    GameService.Content.PlaySoundEffectByName("error");
                    return;
                }

                isRefreshing = true;

                // Don't use this as reference. Changing current view from inside current view like the following is a sin.
                // A more clean approach would be if this refresh button was outside this view and not be refreshed with it.
                // However, there is no space so we doing this quick and dirty.
                var basicTooltipText = new AsyncString();
                var loadingText      = new AsyncString();
                ((ViewContainer)buildPanel).Show(new LoadingView("Refreshing…", loadingText, basicTooltipText));

                if (!await ProofLogix.Instance.KpWebApi.Refresh(_profile.Id)) {
                    GameService.Content.PlaySoundEffectByName("error");
                    ProofLogix.Logger.Warn($"Refresh for '{_profile.Id}' failed - perhaps user API key is bad or API is down.");
                    ScreenNotification.ShowNotification("Refresh failed. Please, try again.", ScreenNotification.NotificationType.Error);
                    ProfileView.Open(_profile);
                    return;
                }

                var retries = 60;
                var timer = new Timer(1250);
                timer.Elapsed += async (_, _) => {
                    if (retries <= 0) {
                        ProfileView.Open(await ProofLogix.Instance.KpWebApi.GetProfile(_profile.Id));
                        timer.Stop();
                        timer.Dispose();
                        return;
                    }

                    retries--;

                    var retryStr = $"({60 - retries} / 60)";
                    basicTooltipText.String = $"Checking completion… {retryStr}";
                    loadingText.String = ProofLogix.Instance.Resources.GetLoadingSubtitle();

                    if (await ProofLogix.Instance.KpWebApi.IsProofBusy(_profile.Id)) {
                        return;
                    }

                    await Task.Delay(1000).ContinueWith(async _ => {
                        var profile = await ProofLogix.Instance.KpWebApi.GetProfile(_profile.Id);
                        ProofLogix.Instance.PartySync.AddKpProfile(profile);
                        GameService.Content.PlaySoundEffectByName("color-change");
                        ProfileView.Open(profile);
                    });

                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            };

            base.Build(buildPanel);
        }

        private sealed class ItemsView : View {

            private readonly Profile   _profile;

            public ItemsView(Profile profile) {
                _profile   = profile;
            }

            protected override void Build(Container buildPanel) {

                if (_profile.IsEmpty) {
                    var nothingFound = "Nothing found.";
                    var description  = "\n  Player is registered but either has proofs explicitly hidden or none at all.";
                    var fontSize     = ContentService.FontSize.Size24;
                    var labelSize = LabelUtil.GetLabelSize(fontSize, nothingFound + description, true);
                    var label = new FormattedLabelBuilder().SetHeight(labelSize.Y).SetWidth(labelSize.X)
                                                           .CreatePart(nothingFound, o => {
                                                                o.SetFontSize(fontSize);
                                                                o.SetPrefixImage(GameService.Content.GetTexture("common/1444522"));
                                                                o.SetTextColor(Color.Yellow);
                                                            }).CreatePart(description, o => {
                                                                o.SetFontSize(ContentService.FontSize.Size18);
                                                                o.SetTextColor(Color.White);
                                                            }).Build();
                    label.Parent = buildPanel;
                    return;
                }

                var menuPanel = new Panel {
                    Parent    = buildPanel,
                    Width     = 200,
                    Height    = buildPanel.ContentRegion.Height,
                    CanScroll = true,
                    Title     = "Mode"
                };

                var navMenu = new Menu {
                    Parent = menuPanel,
                    Top    = 0,
                    Left   = 0,
                    Width  = menuPanel.ContentRegion.Width,
                    Height = menuPanel.ContentRegion.Height,
                };

                var fractalsEntry = new MenuItem {
                    Parent           = navMenu,
                    Text             = "Fractals",
                    Width            = navMenu.ContentRegion.Width,
                    Icon             = GameService.Content.DatAssetCache.GetTextureFromAssetId(514379),
                    BasicTooltipText = "Rewards related to Fractals including tokens and titles."
                };

                var raidsEntry = new MenuItem {
                    Parent           = navMenu,
                    Text             = "Raids",
                    Width            = navMenu.ContentRegion.Width,
                    Icon             = GameService.Content.DatAssetCache.GetTextureFromAssetId(1128644),
                    BasicTooltipText = "Rewards related to Raids including tokens and titles."
                };

                var strikesEntry = new MenuItem {
                    Parent           = navMenu,
                    Text             = "Strikes",
                    Width            = navMenu.ContentRegion.Width,
                    Icon             = GameService.Content.DatAssetCache.GetTextureFromAssetId(2200049),
                    BasicTooltipText = "Rewards related to Strike Missions including tokens and titles."
                };

                var plyPanel = new ViewContainer {
                    Parent     = buildPanel,
                    Left       = menuPanel.Right,
                    Width      = buildPanel.ContentRegion.Width - menuPanel.Width,
                    Height     = buildPanel.ContentRegion.Height,
                    ShowBorder = true
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
                    fractalsEntry.Width = e.CurrentRegion.Width;
                    raidsEntry.Height   = e.CurrentRegion.Height;
                    strikesEntry.Height = e.CurrentRegion.Height;
                };


                fractalsEntry.ItemSelected += (_, _) => {
                    var fractalResources = ProofLogix.Instance.Resources.GetItemsForFractals();
                    var fractalTokens    = _profile.Totals.GetTokens().Where(token => fractalResources.Any(res => res.Id == token.Id)).ToList();
                    var fractalsResults  = new ProfileItems(_profile.Totals.Titles.Where(title => title.Mode == TitleMode.Fractal), fractalTokens);
                    ProofLogix.Instance.ProfileConfig.Value.SelectedTab = 0;
                    plyPanel.Show(new ProfileItemsView(fractalsResults));
                };

                raidsEntry.ItemSelected += (_, _) => {
                    var raidResources = ProofLogix.Instance.Resources.GetItems(Resources.LEGENDARY_INSIGHT, Resources.LEGENDARY_DIVINATION, Resources.BANANAS, Resources.BANANAS_IN_BULK)
                                                  .Concat(ProofLogix.Instance.Resources.GetItemsForRaids());
                    var raidTokens = _profile.Totals.GetTokens().Where(token => raidResources.Any(res => res.Id == token.Id));
                    var raidResults   = new ProfileItems(_profile.Totals.Titles.Where(title => title.Mode == TitleMode.Raid), raidTokens);
                    ProofLogix.Instance.ProfileConfig.Value.SelectedTab = 1;
                    plyPanel.Show(new ProfileItemsView(raidResults));
                };

                strikesEntry.ItemSelected += (_, _) => {
                    var strikeResources = ProofLogix.Instance.Resources.GetItems(Resources.LEGENDARY_INSIGHT, Resources.BONESKINNER_RITUAL_VIAL)
                                                    .Concat(ProofLogix.Instance.Resources.GetItemsForStrikes());
                    var strikeTokens    = _profile.Totals.GetTokens().Where(token => strikeResources.Any(res => res.Id == token.Id));
                    var strikeResults   = new ProfileItems(_profile.Totals.Titles.Where(title => title.Mode == TitleMode.Strike), strikeTokens);
                    ProofLogix.Instance.ProfileConfig.Value.SelectedTab = 2;
                    plyPanel.Show(new ProfileItemsView(strikeResults, true));
                };
                ((MenuItem)navMenu.Children[ProofLogix.Instance.ProfileConfig.Value.SelectedTab]).Select();
            }
        }

        private sealed class ProfileItems {

            public IReadOnlyList<Title> Titles { get; init; }

            public IReadOnlyList<Token> Tokens { get; init; }

            public ProfileItems(IEnumerable<Title> titles, IEnumerable<Token> tokens) {
                Titles = titles.ToList();
                Tokens = tokens.ToList();
            }
        }

        private sealed class ProfileItemsView : View {

            private          ProfileItems _items;
            private readonly Texture2D    _iconTitle;
            private          bool         _displayAsText;
            public ProfileItemsView(ProfileItems items, bool displayAsText = false) {
                _items     = items;
                _iconTitle = ProofLogix.Instance.ContentsManager.GetTexture("icon_title.png");
                _displayAsText = displayAsText;
            }

            protected override void Unload() {
                _iconTitle.Dispose();
                base.Unload();
            }

            protected override void Build(Container buildPanel) {
                var panel = new FlowPanel {
                    Parent              = buildPanel,
                    Width               = buildPanel.ContentRegion.Width,
                    Height              = buildPanel.ContentRegion.Height,
                    FlowDirection       = ControlFlowDirection.LeftToRight,
                    OuterControlPadding = new Vector2(5, 5),
                    ControlPadding      = new Vector2(5, 5),
                    CanScroll = true
                };

                buildPanel.ContentResized += (_, e) => {
                    panel.Width = e.CurrentRegion.Width;
                    panel.Height = e.CurrentRegion.Height;
                };

                foreach (var token in _items.Tokens) {
                    if (token.Amount <= 0) {
                        continue;
                    }
                    if (_displayAsText) {
                        var item = ProofLogix.Instance.Resources.GetItem(token.Id);
                        var text = ' ' + AssetUtil.GetItemDisplayName(token.Name, token.Amount, false);
                        var size = LabelUtil.GetLabelSize(ContentService.FontSize.Size20, text, true);
                        var label = new FormattedLabelBuilder()
                                   .SetWidth(panel.ContentRegion.Width)
                                   .SetHeight(size.Y)
                                   .CreatePart(text, o => {
                                        o.SetTextColor(item.Rarity.AsColor());
                                        o.SetFontSize(ContentService.FontSize.Size20);
                                        o.SetPrefixImage(item.Icon);
                                        o.SetPrefixImageSize(new Point(size.Y, size.Y));
                                    }).Build();
                        label.Parent = panel;
                    } else {
                        ItemWithAmount.Create(token.Id, token.Amount).Parent = panel;
                    }
                }

                foreach (var title in _items.Titles) {
                    var text = ' ' + title.Name;
                    var size = LabelUtil.GetLabelSize(ContentService.FontSize.Size20, text, true);

                    var label = new FormattedLabelBuilder()
                               .SetWidth(panel.ContentRegion.Width)
                               .SetHeight(size.Y)
                               .CreatePart(text, o => {
                                   o.SetFontSize(ContentService.FontSize.Size20);
                                   o.SetPrefixImage(_iconTitle);
                                   o.SetPrefixImageSize(new Point(size.Y, size.Y));
                               })
                               .Build();
                    label.Parent = panel;
                }
            }
        }
    }
}
