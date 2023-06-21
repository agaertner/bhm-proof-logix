using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.ProofLogix.Core.Services;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using Nekres.ProofLogix.Core.Services.PartySync.Models;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.ProofLogix.Core.UI.Table {
    public class TablePresenter : Presenter<TableView, TableConfig> {

        public TablePresenter(TableView view, TableConfig model) : base(view, model) {
            PartySyncService.OnPlayerAdded   += OnPlayerAddedOrChanged;
            PartySyncService.OnPlayerChanged += OnPlayerAddedOrChanged;
            PartySyncService.OnPlayerRemoved += OnPlayerRemoved;
        }

        public void AddPlayer(Player player) {
            var key = player.AccountName?.ToLowerInvariant();

            if (!player.HasKpProfile || string.IsNullOrEmpty(key)) {
                return;
            }

            if (player.KpProfile.NotFound) {
                return;
            }

            var size = LabelUtil.GetLabelSize(this.View.Table.Font, player.AccountName, true);
            var accountName = new FormattedLabelBuilder()
                       .SetWidth(size.X).SetHeight(size.Y)
                       .SetHorizontalAlignment(HorizontalAlignment.Center)
                       .CreatePart(player.AccountName, o => { 
                                  o.SetFontSize(ContentService.FontSize.Size16);

                                  if (player.KpProfile.NotFound) {
                                      o.SetPrefixImage(GameService.Content.GetTexture("common/1444522"));
                                      return;
                                  }

                                  o.SetLink(() => OpenProfileWindow(player.KpProfile));

                              }).Build();

            accountName.BasicTooltipText = !player.KpProfile.NotFound ? player.KpProfile.ProofUrl : string.Empty;
            accountName.Parent           = this.View.Table;
            accountName.Visible          = false;

            var totals = player.KpProfile.Totals;

            var row = new List<object> {
                player.Icon, player.CharacterName, accountName
            };

            var tokens = ResourceService.GetItemsForFractals()
                                        .Union(ResourceService.GetGeneralItems())
                                        .Union(ResourceService.GetItemsForMap(GameService.Gw2Mumble.CurrentMap.Id))
                                        .Select(i => totals.GetToken(i.Id)?.Amount).Cast<object>();
            
            row.AddRange(tokens);

            UpdateHeader();
            this.View.Table.ChangeData(key, row.ToArray());
        }

        public void UpdateHeader() {
            var row = new List<object> {
                string.Empty, "Character", "Account"
            };

            var tokens = ResourceService.GetItemsForFractals()
                                        .Union(ResourceService.GetGeneralItems())
                                        .Union(ResourceService.GetItemsForMap(GameService.Gw2Mumble.CurrentMap.Id))
                                        .Select(item => item.Icon).Cast<object>();

            row.AddRange(tokens);

            this.View.Table.ChangeHeader(row.ToArray());
        }

        private void OpenProfileWindow(Profile profile) {
            var key = profile.Name.ToLowerInvariant();

            if (TrackableWindow.TryGetById(key, out var wnd)) {
                wnd.Left = (GameService.Graphics.SpriteScreen.Width  - wnd.Width)  / 2;
                wnd.Top  = (GameService.Graphics.SpriteScreen.Height - wnd.Height) / 2;
                wnd.BringWindowToFront();
                wnd.Show();
                return;
            }

            var window = new TrackableWindow(key, GameService.Content.DatAssetCache.GetTextureFromAssetId(155985),
                                             new Rectangle(40,  26, 913, 691),
                                             new Rectangle(70, 36, 839, 605)) {
                Parent    = GameService.Graphics.SpriteScreen,
                Title     = $"Profile: {profile.Name}",
                Subtitle  = "Kill Proof",
                CanResize = true,
                Width     = 700,
                Height    = 600,
                Left = (GameService.Graphics.SpriteScreen.Width - 700) / 2,
                Top = (GameService.Graphics.SpriteScreen.Height - 600) / 2,
                Emblem = GameService.Content.GetTexture("common/733268")
            };

            window.Show(new LinkedView(profile));
        }

        protected override void Unload() {
            PartySyncService.OnPlayerAdded   -= OnPlayerAddedOrChanged;
            PartySyncService.OnPlayerChanged -= OnPlayerAddedOrChanged;
            PartySyncService.OnPlayerRemoved -= OnPlayerRemoved;
            base.Unload();
        }

        private void OnPlayerRemoved(object sender, ValueEventArgs<Player> e) {
            var key = e.Value.AccountName?.ToLowerInvariant();

            if (string.IsNullOrEmpty(key)) {
                return;
            }

            this.View.Table.RemoveData(key);
        }

        private void OnPlayerAddedOrChanged(object sender, ValueEventArgs<Player> e) {
            this.AddPlayer(e.Value);
        }
    }
}
