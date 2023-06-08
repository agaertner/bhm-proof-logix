using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.ProofLogix.Core.Services;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using System.Collections.Generic;
using System.Linq;
using static Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models.Title;
using Profile = Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models.Profile;
using Title = Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models.Title;
using Token = Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models.Token;

namespace Nekres.ProofLogix.Core.UI.Table {
    public class ProfileView : View {

        private readonly Profile _profile;

        public ProfileView(Profile profile) {
            _profile = profile;
        }

        protected override void Build(Container buildPanel) {
            var header = new FlowPanel {
                Parent              = buildPanel,
                Width               = buildPanel.ContentRegion.Width,
                Height              = 100,
                ControlPadding      = new Vector2(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                FlowDirection       = ControlFlowDirection.SingleTopToBottom,
                ShowBorder          = true
            };

            var info = new FlowPanel {
                Parent              = header,
                Width               = header.Width,
                Height              = header.ContentRegion.Height - 45,
                ControlPadding      = new Vector2(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                FlowDirection       = ControlFlowDirection.SingleTopToBottom
            };

            var navMenu = new FlowPanel {
                Parent              = header,
                Width               = header.ContentRegion.Width,
                Height              = 45,
                ControlPadding      = new Vector2(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                FlowDirection       = ControlFlowDirection.SingleRightToLeft,
            };

            var lastRefreshText = _profile.LastRefresh.AsTimeAgo();
            var size            = LabelUtil.GetLabelSize(ContentService.FontSize.Size11, lastRefreshText);
            var lastRefresh     = new FormattedLabelBuilder().SetWidth(size.X).SetHeight(size.Y)
                                                             .CreatePart(lastRefreshText, o => {
                                                                  o.SetFontSize(ContentService.FontSize.Size11);
                                                                  o.MakeItalic();
                                                              }).Build();
            lastRefresh.Parent = info;

            header.ContentResized += (_, e) => {
                info.Width    = e.CurrentRegion.Width;
                info.Height   = e.CurrentRegion.Height - navMenu.Height;
                navMenu.Width = e.CurrentRegion.Width;
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

        private sealed class ClearsView : View {

            private readonly IReadOnlyList<Clear> _clears;

            private readonly Texture2D _greenTick;
            private readonly Texture2D _redCross;

            public ClearsView(IEnumerable<Clear> clears) {
                _clears    = clears.ToList();
                _greenTick = ProofLogix.Instance.ContentsManager.GetTexture("green-tick.gif");
                _redCross  = ProofLogix.Instance.ContentsManager.GetTexture("red-cross.gif");
            }

            protected override void Unload() {
                _greenTick.Dispose();
                _redCross.Dispose();
                base.Unload();
            }

            protected override void Build(Container buildPanel) {
                var panel = new FlowPanel {
                    Parent        = buildPanel,
                    Width         = buildPanel.ContentRegion.Width,
                    Height        = buildPanel.ContentRegion.Height,
                    CanScroll     = true,
                    FlowDirection = ControlFlowDirection.SingleTopToBottom
                };

                buildPanel.ContentResized += (_, e) => {
                    panel.Width  = e.CurrentRegion.Width;
                    panel.Height = e.CurrentRegion.Height;
                };

                foreach (var clear in _clears) {

                    var wing = new FlowPanel {
                        Parent           = panel,
                        Width            = panel.ContentRegion.Width,
                        HeightSizingMode = SizingMode.AutoSize,
                        Title            = clear.Name,
                        CanCollapse      = true,
                        FlowDirection    = ControlFlowDirection.SingleTopToBottom
                    };

                    panel.ContentResized += (_, e) => {
                        wing.Width  = e.CurrentRegion.Width;
                    };

                    foreach (var encounter in clear.Encounters) {

                        var icon = encounter.Cleared ? _greenTick : _redCross;
                        var size = LabelUtil.GetLabelSize(ContentService.FontSize.Size16, encounter.Name, true);

                        var encounterItem = new FormattedLabelBuilder()
                                           .SetWidth(size.X)
                                           .SetHeight(size.Y + Control.ControlStandard.ControlOffset.Y)
                                           .CreatePart(encounter.Name, o => { 
                                                o.SetFontSize(ContentService.FontSize.Size16); 
                                                o.SetPrefixImage(icon);
                                            }).Build();

                        encounterItem.Parent = wing;
                    }
                }
                base.Build(buildPanel);
            }

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
                    Parent        = buildPanel,
                    Width         = buildPanel.ContentRegion.Width,
                    Height        = buildPanel.ContentRegion.Height,
                    FlowDirection = ControlFlowDirection.LeftToRight
                };

                buildPanel.ContentResized += (_, e) => {
                    panel.Width  = e.CurrentRegion.Width;
                    panel.Height = e.CurrentRegion.Height;
                };

                var totals = _profile.Totals;

                var fractalResources = ResourceService.GetItemsForFractals();

                var tokens        = totals.GetTokens().ToList();
                var fractalTokens = tokens.Where(token => fractalResources.Any(res => res.Id == token.Id));
                var raidTokens = tokens.Where(token => fractalResources.Any(res => res.Id           != token.Id)
                                                    && ResourceService.ObsoleteItemIds.All(id => id != token.Id));

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
                    var icon = ResourceService.GetItem(token.Id)?.Icon ?? ResourceService.GetApiIcon(token.Id);

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
