using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.ProofLogix.Core.Services;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using Nekres.ProofLogix.Core.Services.PartySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            var size = GetLabelSize(player.AccountName, true);
            var accountName = new FormattedLabelBuilder()
                       .SetWidth(size.X).SetHeight(size.Y)
                       .SetHorizontalAlignment(HorizontalAlignment.Center)
                       .CreatePart(player.AccountName, o => { 
                                  o.SetFontSize(ContentService.FontSize.Size16);

                                  if (player.KpProfile.IsEmpty) {
                                      o.SetPrefixImage(GameService.Content.GetTexture("common/1444522"));
                                      return;
                                  }

                                  o.SetHyperLink(player.KpProfile.ProofUrl);

                              }).Build();

            accountName.BasicTooltipText = !player.KpProfile.IsEmpty ? player.KpProfile.ProofUrl : string.Empty;
            accountName.Parent           = this.View.Table;
            accountName.Visible          = this.View.Table.Visible;

            var totals = (IProfileV2)player.KpProfile.LinkedTotals ?? player.KpProfile;

            var row = new List<object> {
                player.Icon, player.CharacterName, accountName
            };

            var tokens = ResourceService.GetItemIds().Select(i => totals.GetToken(i)?.Amount).Cast<object>();
            
            row.AddRange(tokens);

            this.View.Table.ChangeData(key, row.ToArray());
        }

        /// <summary>
        /// Workaround until <see cref="FormattedLabelBuilder.AutoSizeHeight"/> and <see cref="FormattedLabelBuilder.AutoSizeWidth"/> is fixed.
        /// </summary>
        private Point GetLabelSize(string text, bool hasPrefix = false, bool hasSuffix = false) {
            var icon = this.View.Table.Font.MeasureString("."); // Additional measurement of a single glyph for icons since the text might contain line breaks.
            var size = this.View.Table.Font.MeasureString(text);

            float width;

            if (hasPrefix && hasSuffix) {
                width = size.Width + icon.Height * 4;
            } else if (hasPrefix || hasSuffix) {
                width = size.Width + icon.Height * 2;
            } else {
                width = size.Width;
            }

            return new Point((int)width, (int)size.Height);
        }

        protected override Task<bool> Load(IProgress<string> progress) {
            return base.Load(progress);
        }

        protected override void UpdateView() {

            base.UpdateView();
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
