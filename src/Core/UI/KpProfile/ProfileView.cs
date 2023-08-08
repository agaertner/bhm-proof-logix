using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Extended;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
                ((ViewContainer)buildPanel).Show(new LoadingView("Refreshing...", loadingText, basicTooltipText));

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
                    basicTooltipText.String = $"Checking completion.. {retryStr}";
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
                var raidTokens    = tokens.Where(token => fractalResources.All(res => res.Id != token.Id));

                var fractalResults = new ProfileItems(totals.Titles.Where(title => title.Mode == TitleMode.Fractal), fractalTokens);
                var raidResults    = new ProfileItems(totals.Titles.Where(title => title.Mode == TitleMode.Raid),    raidTokens);

                CreateItemPanel(panel, fractalResults, "Fractals");
                CreateItemPanel(panel, raidResults,    "Raids");

                base.Build(panel);
            }

            private void CreateItemPanel(Container parent, ProfileItems items, string panelTitle) {
                var flow = new FlowPanel {
                    Parent              = parent,
                    Width               = parent.ContentRegion.Width / 2 - Panel.RIGHT_PADDING,
                    Height              = parent.ContentRegion.Height    - Panel.RIGHT_PADDING,
                    ControlPadding      = new Vector2(Control.ControlStandard.ControlOffset.X, Control.ControlStandard.ControlOffset.Y),
                    OuterControlPadding = new Vector2(Control.ControlStandard.ControlOffset.X, Control.ControlStandard.ControlOffset.Y),
                    FlowDirection       = ControlFlowDirection.SingleTopToBottom,
                    CanScroll           = true,
                    Title               = panelTitle
                };

                parent.ContentResized += (_, e) => {
                    flow.Height = e.CurrentRegion.Height    - Panel.RIGHT_PADDING;
                    flow.Width  = e.CurrentRegion.Width / 2 - Panel.RIGHT_PADDING;
                };

                foreach (var title in items.Titles) {

                    var text = ' ' + title.Name;
                    var size = LabelUtil.GetLabelSize(ContentService.FontSize.Size20, text, true);

                    var label = new FormattedLabelBuilder()
                               .SetWidth(size.X)
                               .SetHeight(size.Y)
                               .CreatePart(text, o => {
                                   o.SetFontSize(ContentService.FontSize.Size20);
                                   o.SetPrefixImage(_iconTitle);
                                   o.SetPrefixImageSize(new Point(size.Y, size.Y));
                               })
                               .Build();
                    label.Parent = flow;
                }

                foreach (var token in items.Tokens) {
                    if (token.Amount == 0) {
                        continue;
                    }

                    var text = ' ' + AssetUtil.GetItemDisplayName(token.Name, token.Amount);
                    var size = LabelUtil.GetLabelSize(ContentService.FontSize.Size20, text, true);
                    var icon = ProofLogix.Instance.Resources.GetItem(token.Id).Icon;

                    var label = new FormattedLabelBuilder()
                               .SetWidth(size.X)
                               .SetHeight(size.Y)
                               .CreatePart(text, o => {
                                    o.SetTextColor(ProofLogix.Instance.Resources.GetItem(token.Id).Rarity.AsColor());
                                    o.SetFontSize(ContentService.FontSize.Size20);
                                    o.SetPrefixImage(icon);
                                    o.SetPrefixImageSize(new Point(size.Y, size.Y));
                                }).Build();
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
