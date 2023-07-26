using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.ProofLogix.Core.UI.Configs;
using System;
using System.Linq;

namespace Nekres.ProofLogix.Core.UI.Table {
    public class TableView : View<TablePresenter> {

        public FlowPanel Table;

        public TableView(TableConfig config) {
            this.WithPresenter(new TablePresenter(this, config));
        }

        protected override void Build(Container buildPanel) {

            ((WindowBase2)buildPanel).Subtitle = "Squad Tracker";

            var search = new TextBox {
                Parent = buildPanel,
                Width = 200,
                Height = 32,
                Font = GameService.Content.DefaultFont18,
                PlaceholderText = "Search..",
                BasicTooltipText = "Guild Wars 2 account, character name or \nKillproof.me identifier."
            };

            var notFound = "Not found.";
            var size     = LabelUtil.GetLabelSize(GameService.Content.DefaultFont18, notFound, true);
            var notFoundLabel = new FormattedLabelBuilder()
                               .SetWidth(size.X)
                               .SetHeight(size.Y)
                               .CreatePart(notFound, o => {
                                    o.SetTextColor(Color.Yellow);
                                    o.SetPrefixImage(GameService.Content.GetTexture("common/1444522"));
                                }).Build();

            notFoundLabel.Parent  = buildPanel;
            notFoundLabel.Left    = search.Right + Panel.RIGHT_PADDING;
            notFoundLabel.Top     = search.Top   + (search.Height - notFoundLabel.Height) / 2;
            notFoundLabel.Visible = false;

            var loading = new LoadingSpinner {
                Parent  = buildPanel,
                Width   = size.Y,
                Height  = size.Y,
                Left    = search.Right + Panel.RIGHT_PADDING,
                Top     = search.Top   + (search.Height - size.Y) / 2,
                Visible = false
            };

            search.EnterPressed += async (_, _) => {
                if (string.IsNullOrEmpty(search.Text)) {
                    return;
                }

                loading.Visible = true;

                var query   = (string)search.Text.Clone();

                var profile = await ProofLogix.Instance.KpWebApi.GetProfile(query);

                if (profile.NotFound) {
                    profile = await ProofLogix.Instance.KpWebApi.GetProfileByCharacter(query);
                }

                if (profile.NotFound) {

                    loading.Visible = false;

                    notFoundLabel.Visible = search.Text.Equals(query);

                    ScreenNotification.ShowNotification(notFound, ScreenNotification.NotificationType.Warning);
                    GameService.Content.PlaySoundEffectByName("error");
                    return;
                }

                loading.Visible = false;

                if (search.Text.Equals(query)) {
                    search.Text = string.Empty;
                }
                
                ProofLogix.Instance.PartySync.AddKpProfile(profile);
            };

            search.TextChanged += (_, _) => {
                notFoundLabel.Visible = false;
            };

            this.Table = new FlowPanel {
                Parent = buildPanel,
                Top = search.Bottom + Panel.TOP_PADDING,
                Width  = buildPanel.ContentRegion.Width,
                Height = buildPanel.ContentRegion.Height - search.Height - Panel.TOP_PADDING,
                CanScroll = true,
                ControlPadding = new Vector2(5,5),
                OuterControlPadding = new Vector2(5,5),
                FlowDirection = ControlFlowDirection.SingleTopToBottom
            };

            buildPanel.ContentResized += (_, e) => {
                this.Table.Width  = e.CurrentRegion.Width;
                this.Table.Height = e.CurrentRegion.Height - search.Height - Panel.TOP_PADDING;
            };

            var headerEntry = new TableHeaderEntry {
                Parent = this.Table,
                Width  = this.Table.ContentRegion.Width,
                Height = 32
            };

            this.Table.ContentResized += (_, e) => {
                headerEntry.Width = e.CurrentRegion.Width;
            };

            headerEntry.ColumnClick += HeaderEntry_ColumnClick;
            this.Presenter.AddPlayer(ProofLogix.Instance.PartySync.LocalPlayer);
            foreach (var player in ProofLogix.Instance.PartySync.PlayerList) {
                this.Presenter.AddPlayer(player);
            }
            this.Presenter.SortEntries();

            base.Build(buildPanel);
        }

        private void HeaderEntry_ColumnClick(object sender, ValueEventArgs<int> e) {
            this.Presenter.Model.SelectedColumn  = e.Value;
            this.Presenter.Model.OrderDescending = !this.Presenter.Model.OrderDescending;
            this.Presenter.SortEntries();
        }
    }
}
