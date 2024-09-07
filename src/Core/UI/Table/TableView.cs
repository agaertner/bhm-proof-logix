using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Extended;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.ProofLogix.Core.Services;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using Nekres.ProofLogix.Core.UI.Configs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.ProofLogix.Core.UI.Table {
    public class TableView : View<TablePresenter> {

        public FlowPanel Table          { get; private set; }
        public Label     PlayerCountLbl { get; private set; }

        private AsyncTexture2D _cogWheelIcon;
        private AsyncTexture2D _cogWheelIconHover;
        private AsyncTexture2D _cogWheelIconClick;

        private const int SCROLLBAR_WIDTH = 12;

        public TableView(TableConfig config) {
            this.WithPresenter(new TablePresenter(this, config));
            _cogWheelIcon      = GameService.Content.DatAssetCache.GetTextureFromAssetId(155052);
            _cogWheelIconHover = GameService.Content.DatAssetCache.GetTextureFromAssetId(157110);
            _cogWheelIconClick = GameService.Content.DatAssetCache.GetTextureFromAssetId(157109);
        }

        protected override void Build(Container buildPanel) {
            var cogWheel = new Image(_cogWheelIcon) {
                Parent = buildPanel,
                Width = 32,
                Height = 32
            };

            cogWheel.MouseEntered += (_, _) => {
                cogWheel.Texture = _cogWheelIconHover;
            };

            cogWheel.MouseLeft += (_, _) => {
                cogWheel.Texture = _cogWheelIcon;
            };

            cogWheel.LeftMouseButtonPressed += (_, _) => {
                cogWheel.Texture = _cogWheelIconClick;
            };

            cogWheel.LeftMouseButtonReleased += (_, _) => {
                cogWheel.Texture = _cogWheelIconHover;
            };

            var search = new TextBox {
                Parent           = buildPanel,
                Width            = 200,
                Height           = 32,
                Left             = cogWheel.Right + Control.ControlStandard.ControlOffset.X,
                Font             = GameService.Content.DefaultFont18,
                PlaceholderText  = "Search..",
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

            var headerEntry = new TableHeaderEntry {
                Parent = buildPanel,
                Top    = search.Bottom + Panel.TOP_PADDING,
                Height = 32
            };

            this.PlayerCountLbl = new Label {
                Parent              = buildPanel,
                Text                = string.Empty,
                Top                 = buildPanel.ContentRegion.Height - 24,
                Width               = buildPanel.ContentRegion.Width  - 24,
                Height              = 24,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment   = VerticalAlignment.Middle,
                BasicTooltipText    = "Playercount"
            };

            var playerIconTex = GameService.Content.DatAssetCache.GetTextureFromAssetId(733268);
            var playerIconImg = new Image(playerIconTex) {
                Parent = buildPanel,
                Width  = 24,
                Height = 24,
                Top    = this.PlayerCountLbl.Top,
                Left   = this.PlayerCountLbl.Right
            };

            this.Table = new FlowPanel {
                Parent         = buildPanel,
                Top            = headerEntry.Bottom              + Control.ControlStandard.ControlOffset.Y,
                Width          = headerEntry.Width               + SCROLLBAR_WIDTH,
                Height         = buildPanel.ContentRegion.Height - search.Height - headerEntry.Height - Panel.TOP_PADDING - Control.ControlStandard.ControlOffset.Y - this.PlayerCountLbl.Height,
                CanScroll      = true,
                ControlPadding = new Vector2(0,Panel.BOTTOM_PADDING),
                FlowDirection  = ControlFlowDirection.SingleTopToBottom
            };

            headerEntry.Resized += (_, e) => {
                this.Table.Width = e.CurrentSize.X  + SCROLLBAR_WIDTH;
                buildPanel.Width = this.Table.Width + (buildPanel.Width - buildPanel.ContentRegion.Width) - SCROLLBAR_WIDTH / 2;
            };

            buildPanel.ContentResized += (_, e) => {
                this.Table.Height         = e.CurrentRegion.Height          - search.Height - headerEntry.Height - Panel.TOP_PADDING - Control.ControlStandard.ControlOffset.Y - this.PlayerCountLbl.Height;
                this.PlayerCountLbl.Top   = buildPanel.ContentRegion.Height - 24;
                this.PlayerCountLbl.Width = buildPanel.ContentRegion.Width  - 24;
                playerIconImg.Top         = this.PlayerCountLbl.Top;
                playerIconImg.Left        = this.PlayerCountLbl.Right;
            };

            headerEntry.ColumnClick += (_, e) => {
                this.Presenter.Model.SelectedColumn  = e.Value;
                this.Presenter.Model.OrderDescending = !this.Presenter.Model.OrderDescending;
                this.Presenter.SortEntries();
            };

            var menu = new ContextMenuStrip {
                Parent = buildPanel,
                ClipsBounds = false
            };

            var alwaysSortStatusEntry = new ContextMenuStripItem("Always Sort by Status") {
                Parent = menu,
                CanCheck = true,
                Checked = this.Presenter.Model.AlwaysSortStatus,
                BasicTooltipText = "Sorts players by their online status\nbefore sorting by your selected column."
            };

            alwaysSortStatusEntry.CheckedChanged += (_, e) => {
                this.Presenter.Model.AlwaysSortStatus = e.Checked;
                this.Presenter.SortEntries();
            };

            var keepLeaversEntry = new ContextMenuStripItem("Keep Leavers") {
                Parent           = menu,
                CanCheck         = true,
                Checked          = this.Presenter.Model.KeepLeavers,
                BasicTooltipText = "Disables the automatic removal of players who have left your party."
            };

            keepLeaversEntry.CheckedChanged += (_, e) => {
                this.Presenter.Model.KeepLeavers = e.Checked;
            };

            var requireProfileEntry = new ContextMenuStripItem("Require Profile") {
                Parent = menu,
                CanCheck = true,
                Checked = this.Presenter.Model.RequireProfile,
                BasicTooltipText = "Only players with a profile are added when they join your party."
            };

            requireProfileEntry.CheckedChanged += (_, e) => {
                this.Presenter.Model.RequireProfile = e.Checked;
            };

            var colorGradingModeCategory = new ContextMenuStripItem("Color Grading Mode") {
                Parent = menu,
                Submenu = new ContextMenuStrip()
            };

            var colorGradingModeEntries = new List<ContextMenuStripItem>();
            foreach (var mode in Enum.GetValues(typeof(PartySyncService.ColorGradingMode)).Cast<PartySyncService.ColorGradingMode>()) {

                var suffixTooltip = mode switch {
                    PartySyncService.ColorGradingMode.LocalPlayerComparison => "your own",
                    PartySyncService.ColorGradingMode.MedianComparison      => "the median",
                    PartySyncService.ColorGradingMode.LargestComparison     => "the largest",
                    PartySyncService.ColorGradingMode.AverageComparison     => "the average",
                    _                                                       => string.Empty
                };

                var colorGradingMode = new ContextMenuStripItem(mode.ToString().SplitCamelCase()) {
                    Parent   = colorGradingModeCategory.Submenu,
                    CanCheck = true,
                    Checked = this.Presenter.Model.ColorGradingMode == mode,
                    BasicTooltipText = $"Highlight low amounts by comparison to {suffixTooltip}."
                };

                colorGradingMode.CheckedChanged += (o, e) => {
                    if (!e.Checked) {
                        // Immediately rechecks on uncheck which visually disables unchecking.
                        // CheckedChanged is invoked twice due to this lazyness but it gets the job done.
                        if (colorGradingModeEntries.All(x => !x.Checked)) {
                            // If all are unchecked here we know that this was the sole one and needs to be rechecked.
                            colorGradingMode.Checked = true;
                        }
                        return; // Do nothing for the forced unchecks.
                    }
                    // Force uncheck all except click sender.
                    foreach (var entry in colorGradingModeEntries.Where(x => x != o)) {
                        entry.Checked = false;
                    }
                    this.Presenter.Model.ColorGradingMode = mode;
                    GameService.Content.PlaySoundEffectByName("color-change");
                };
                colorGradingModeEntries.Add(colorGradingMode);
            }

            var columnsCategory = new ContextMenuStripItem("Columns") {
                Parent  = menu,
                Submenu = new ContextMenuStrip()
            };

            var identityCategory = new ContextMenuStripItem("Identifier") {
                Parent  = columnsCategory.Submenu,
                Submenu = new ContextMenuStrip()
            };

            foreach (var col in Enum.GetValues(typeof(TableConfig.Column)).Cast<TableConfig.Column>()) {
                var colEntry = new ContextMenuStripItem(col.ToString().SplitCamelCase()) {
                    Parent   = identityCategory.Submenu,
                    CanCheck = true,
                    Checked  = this.Presenter.Model.Columns.Any(c => c == col)
                };
                colEntry.CheckedChanged += (_, e) => {
                    if (e.Checked) {
                        this.Presenter.Model.Columns.Add(col);
                        GameService.Content.PlaySoundEffectByName("color-change");
                    } else {
                        this.Presenter.Model.Columns.RemoveAll(col);
                    }
                };
            }

            var proofsCategory = new ContextMenuStripItem("Proofs") {
                Parent  = columnsCategory.Submenu,
                Submenu = new ContextMenuStrip()
            };

            var generalCategory = new ContextMenuStripItem("Generic") {
                Parent  = proofsCategory.Submenu,
                Submenu = new ContextMenuStrip()
            };

            AddProofEntries(generalCategory,  ProofLogix.Instance.Resources.GetGeneralItems().Where(x => x.Id != Resources.BONESKINNER_RITUAL_VIAL && x.Id != Resources.LEGENDARY_DIVINATION));

            var fractalsCategory = new ContextMenuStripItem("Fractals") {
                Parent  = proofsCategory.Submenu,
                Submenu = new ContextMenuStrip()
            };

            AddProofEntries(fractalsCategory, ProofLogix.Instance.Resources.GetItemsForFractals());

            var raidsCategory = new ContextMenuStripItem("Raids") {
                Parent  = proofsCategory.Submenu,
                Submenu = new ContextMenuStrip()
            };

            AddProofEntries(raidsCategory, ProofLogix.Instance.Resources.GetItems(Resources.LEGENDARY_DIVINATION));

            var coffersCategory = new ContextMenuStripItem("Coffers") {
                Parent  = raidsCategory.Submenu,
                Submenu = new ContextMenuStrip()
            };

            AddProofEntries(coffersCategory, ProofLogix.Instance.Resources.GetCofferItems());

            var i = 1;
            foreach (var wing in ProofLogix.Instance.Resources.GetWings()) {
                var wingEntry = new ContextMenuStripItem($"Wing {i++}") {
                    Parent  = raidsCategory.Submenu,
                    Submenu = new ContextMenuStrip()
                };

                AddProofEntries(wingEntry, wing.Events.Where(ev => ev.Token != null).Select(ev => ev.Token));
            }

            cogWheel.Click += (_, _) => {
                GameService.Content.PlaySoundEffectByName("button-click");
                menu.Show(GameService.Input.Mouse.Position);
            };

            var strikesCategory = new ContextMenuStripItem("Strikes") {
                Parent  = proofsCategory.Submenu,
                Submenu = new ContextMenuStrip()
            };

            AddProofEntries(strikesCategory, ProofLogix.Instance.Resources.GetItems(Resources.BONESKINNER_RITUAL_VIAL)
                                                       .Concat(ProofLogix.Instance.Resources.GetItemsForStrikes()));

            base.Build(buildPanel);
        }


        private void AddProofEntries(ContextMenuStripItem parent, IEnumerable<Resource> resources) {
            foreach (var resource in resources) {
                var tokenEntry = new ContextMenuStripItemWithColor(resource.Name) {
                    Parent   = parent.Submenu,
                    CanCheck = true,
                    Checked  = this.Presenter.Model.TokenIds.Any(id => id == resource.Id),
                    TextColor = ProofLogix.Instance.Resources.GetItem(resource.Id).Rarity.AsColor()
                };
                tokenEntry.CheckedChanged += (_, e) => {
                    if (e.Checked) {
                        this.Presenter.Model.TokenIds.Add(resource.Id);
                        GameService.Content.PlaySoundEffectByName("color-change");
                    } else {
                        this.Presenter.Model.TokenIds.RemoveAll(resource.Id);
                    }
                };
            }
        }
    }
}
