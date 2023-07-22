using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.ProofLogix.Core.UI.Clears;
using System;
using System.Collections.Generic;
using System.Linq;
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

                var entry = new MenuItem(profile.Name) {
                    Parent = menu
                };

                entry.ItemSelected += (_,_) => {
                    if (!_profile.BelongsTo(entry.Text, out var selected)) {
                        return;
                    }
                    profileContainer.Show(new ProfileView(selected));
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

            var key = profile.Name.ToLowerInvariant();

            if (TrackableWindow.TryGetById(key, out var wnd)) {
                wnd.Left = (GameService.Graphics.SpriteScreen.Width  - wnd.Width)  / 2;
                wnd.Top  = (GameService.Graphics.SpriteScreen.Height - wnd.Height) / 2;
                wnd.BringWindowToFront();
                wnd.Show(new LinkedView(profile));
                return;
            }

            var window = new TrackableWindow(key, GameService.Content.DatAssetCache.GetTextureFromAssetId(155985),
                                             new Rectangle(40, 26, 913, 691),
                                             new Rectangle(70, 36, 839, 605)) {
                Parent    = GameService.Graphics.SpriteScreen,
                Title     = $"Profile: {profile.Name}",
                Subtitle  = "Kill Proof",
                Id        = $"{nameof(ProofLogix)}_Profile_a32c972dd9fe4025a01d3256025ab1dc",
                CanResize = true,
                SavesSize = true,
                Width     = 800,
                Height    = 600,
                Left      = (GameService.Graphics.SpriteScreen.Width  - 700) / 2,
                Top       = (GameService.Graphics.SpriteScreen.Height - 600) / 2,
                Emblem    = GameService.Content.GetTexture("common/733268")
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
                Width               = header.ContentRegion.Width / 2,
                Height              = header.ContentRegion.Height,
                ControlPadding      = new Vector2(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                FlowDirection       = ControlFlowDirection.SingleTopToBottom
            };

            var navMenu = new FlowPanel {
                Parent              = header,
                Width               = header.ContentRegion.Width / 2,
                Height              = header.ContentRegion.Height,
                ControlPadding      = new Vector2(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                FlowDirection       = ControlFlowDirection.SingleRightToLeft,
            };

            var nameSize = LabelUtil.GetLabelSize(ContentService.FontSize.Size18, _profile.Name);
            var name = new FormattedLabelBuilder().SetWidth(nameSize.X).SetHeight(nameSize.Y)
                                                  .CreatePart(_profile.Name, o => {
                                                       o.SetFontSize(ContentService.FontSize.Size18);
                                                       o.SetHyperLink(_profile.ProofUrl);
                                                   }).Build();
            name.Parent = info;

            var lastRefreshText = _profile.LastRefresh.AsTimeAgo();
            var size            = LabelUtil.GetLabelSize(ContentService.FontSize.Size11, lastRefreshText);
            var lastRefresh     = new FormattedLabelBuilder().SetWidth(size.X).SetHeight(size.Y)
                                                             .CreatePart(lastRefreshText, o => {
                                                                  o.SetFontSize(ContentService.FontSize.Size11);
                                                                  o.MakeItalic();
                                                              }).Build();
            lastRefresh.Parent = info;

            var refreshBttn = new RefreshButton {
                Parent = info,
                Width  = 32,
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
                ((ViewContainer)buildPanel).Show(new LoadingView("Refreshing..."));

                if (!await ProofLogix.Instance.KpWebApi.Refresh(_profile.Id)) {
                    GameService.Content.PlaySoundEffectByName("error");
                    ProofLogix.Logger.Warn($"Refresh for '{_profile.Id}' failed - perhaps user API key is bad or API is down.");
                    ScreenNotification.ShowNotification("Refresh failed. Please, try again.", ScreenNotification.NotificationType.Error);
                    ProfileView.Open(_profile);
                    return;
                }

                var retries = 60;
                var timer = new Timer(1000);
                timer.Elapsed += async (_, _) => {
                    if (retries <= 0) {
                        ProfileView.Open(await ProofLogix.Instance.KpWebApi.GetProfile(_profile.Id));
                        timer.Stop();
                        timer.Dispose();
                        return;
                    }

                    retries--;

                    if (await ProofLogix.Instance.KpWebApi.IsProofBusy(_profile.Id)) {
                        return;
                    }

                    ProfileView.Open(await ProofLogix.Instance.KpWebApi.GetProfile(_profile.Id));
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            };

            header.ContentResized += (_, e) => {
                info.Width    = e.CurrentRegion.Width / 2;
                info.Height   = e.CurrentRegion.Height;
                navMenu.Width = e.CurrentRegion.Width / 2;
                navMenu.Height = e.CurrentRegion.Height;
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
                body.Show(new ClearsView(_profile.Clears));
            };

            var b2 = new StandardButton {
                Parent = navMenu,
                Width  = 150,
                Height = 30,
                Text   = "Proofs"
            };

            b2.Click += (_, _) => {
                body.Show(new ItemsView(_profile));
            };

            base.Build(buildPanel);
        }

        private sealed class ItemsView : View {

            private readonly Profile   _profile;
            private readonly Texture2D _iconTitle;

            public ItemsView(Profile profile) {
                _profile   = profile;
                _iconTitle = ProofLogix.Instance.ContentsManager.GetTexture("icon_title.png");
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
                    OuterControlPadding = new Vector2(Panel.LEFT_PADDING, Panel.TOP_PADDING)
                };

                buildPanel.ContentResized += (_, e) => {
                    panel.Width  = e.CurrentRegion.Width;
                    panel.Height = e.CurrentRegion.Height;
                };

                if (_profile.IsEmpty) {
                    var nothingFound = "Nothing found.";
                    var description  = "\n  Player is registered but either has proofs explicitly hidden or none at all.";
                    var fontSize     = ContentService.FontSize.Size24;
                    var labelSize    = LabelUtil.GetLabelSize(fontSize, nothingFound + description, true);
                    var label = new FormattedLabelBuilder().SetHeight(labelSize.Y).SetWidth(labelSize.X)
                                                           .CreatePart(nothingFound, o => {
                                                                o.SetFontSize(fontSize);
                                                                o.SetPrefixImage(GameService.Content.GetTexture("common/1444522"));
                                                                o.SetTextColor(Color.Yellow);
                                                            }).CreatePart(description, o => {
                                                                o.SetFontSize(ContentService.FontSize.Size18);
                                                                o.SetTextColor(Color.White);
                                                            }).Build();
                    label.Parent = panel;
                    base.Build(panel);
                    return;
                }
                var totals = _profile.Totals;

                var fractalResources = ProofLogix.Instance.Resources.GetItemsForFractals();

                var tokens        = totals.GetTokens().ToList();
                var fractalTokens = tokens.Where(token => fractalResources.Any(res => res.Id == token.Id));
                var raidTokens = tokens.Where(token => fractalResources.Any(res => res.Id                         != token.Id)
                                                    && ProofLogix.Instance.Resources.ObsoleteItemIds.All(id => id != token.Id));

                var fractalResults = new ProfileItems(totals.Titles.Where(title => title.Mode == TitleMode.Fractal), fractalTokens);
                var raidResults    = new ProfileItems(totals.Titles.Where(title => title.Mode == TitleMode.Raid),    raidTokens);

                CreateItemPanel(panel, fractalResults, "Fractals");
                CreateItemPanel(panel, raidResults,    "Raids");

                base.Build(panel);
            }

            private void CreateItemPanel(Container parent, ProfileItems items, string panelTitle) {
                var padding = 5;
                var flow = new FlowPanel {
                    Parent = parent,
                    Width = parent.ContentRegion.Width / 2 - padding,
                    Height = parent.ContentRegion.Height - padding,
                    ControlPadding = new Vector2(0, padding),
                    OuterControlPadding = new Vector2(0, padding),
                    FlowDirection = ControlFlowDirection.SingleTopToBottom,
                    CanScroll = true,
                    Title = panelTitle
                };

                parent.ContentResized += (_, e) => {
                    flow.Height = e.CurrentRegion.Height - padding;
                    flow.Width = e.CurrentRegion.Width / 2 - padding;
                };

                foreach (var title in items.Titles) {
                    var size = LabelUtil.GetLabelSize(ContentService.FontSize.Size14, title.Name, true);

                    var label = new FormattedLabelBuilder()
                               .SetWidth(size.X)
                               .SetHeight(size.Y)
                               .CreatePart(title.Name, o => {
                                   o.SetFontSize(ContentService.FontSize.Size14);
                                   o.SetPrefixImage(_iconTitle);
                               })
                               .Build();
                    label.Parent = flow;
                }

                foreach (var token in items.Tokens) {
                    if (token.Amount == 0) {
                        continue;
                    }

                    var text = $"{token.Name} x{token.Amount}";
                    var size = LabelUtil.GetLabelSize(ContentService.FontSize.Size14, text, true);
                    var icon = ProofLogix.Instance.Resources.GetResource(token.Id)?.Icon ?? ProofLogix.Instance.Resources.GetApiIcon(token.Id);

                    var label = new FormattedLabelBuilder()
                               .SetWidth(size.X)
                               .SetHeight(size.Y + Control.ControlStandard.ControlOffset.Y)
                               .CreatePart(token.Name, o => {
                                   o.SetFontSize(ContentService.FontSize.Size14);
                                   o.SetPrefixImage(icon);
                               }).CreatePart($"x{token.Amount}", o => {
                                   o.SetFontSize(ContentService.FontSize.Size14);
                                   o.SetTextColor(Color.Green);
                                   o.MakeBold();
                               })
                               .Build();
                    label.Parent = flow;
                }
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
    }
}
