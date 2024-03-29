﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.ProofLogix.Core.Services.PartySync.Models;

namespace Nekres.ProofLogix.Core.UI.Table {
    public class TablePlayerEntry : TableEntryBase {

        private Player _player;
        public Player Player {
            get => _player;
            set => SetProperty(ref _player, value);
        }

        private bool _remember;
        public bool Remember {
            get => _remember;
            set {
                SetProperty(ref _remember, value);
                SetBackgroundColor();
            }
        }

        public TablePlayerEntry(Player player) : base() {
            _player  = player;
        }

        protected override string         Timestamp     => this.Player.Created.ToLocalTime().AsTimeAgo();
        protected override AsyncTexture2D ClassIcon     => this.Player.Icon;
        protected override string         CharacterName => this.Player.CharacterName;
        protected override string         AccountName   => this.Player.AccountName;

        private const int STATUS_ICON_SIZE = 8;

        protected override void OnMouseLeft(MouseEventArgs e) {
            base.OnMouseLeft(e);
            SetBackgroundColor();
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            base.OnMouseEntered(e);
            SetBackgroundColor();
        }

        private void SetBackgroundColor() {
            if (this.IsHovering) {
                this.BackgroundColor = (this.Remember ? Color.LightCyan : Color.LightBlue) * 0.2f;
            } else {
                this.BackgroundColor = this.Remember ? Color.LightGreen * 0.2f : Color.Transparent;
            }
        }

        protected override string GetTimestampTooltip() {
            return this.Player.Created.ToLocalTime().AsRelativeTime();
        }

        protected override string GetClassTooltip() {
            return this.Player.Class;
        }

        protected override string GetCharacterTooltip() {
            return this.Player.CharacterName;
        }

        protected override string GetAccountTooltip() {
            return this.Player.AccountName;
        }

        protected override string GetTokenTooltip(int tokenId) {
            var token = this.Player.KpProfile.GetToken(tokenId);
            return AssetUtil.GetItemDisplayName(token.Name, token.Amount, false);
        }

        protected override void PaintStatus(SpriteBatch spriteBatch, Rectangle bounds) {
            // Keep aspect ratio and center.
            var centered = new Rectangle(bounds.X + (bounds.Width - STATUS_ICON_SIZE) / 2, bounds.Y + (bounds.Height - STATUS_ICON_SIZE) / 2, STATUS_ICON_SIZE, STATUS_ICON_SIZE);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, centered, ProofLogix.Instance.PartySync.GetStatusColor(this.Player.Status));
        }

        protected override void PaintToken(SpriteBatch spriteBatch, Rectangle bounds, int tokenId) {
            var token = this.Player.KpProfile.GetToken(tokenId);
            var color = ProofLogix.Instance.PartySync.GetTokenAmountColor(tokenId, token.Amount, ProofLogix.Instance.TableConfig.Value.ColorGradingMode);
            spriteBatch.DrawStringOnCtrl(this, AssetUtil.Truncate(token.Amount.ToString(), bounds.Width, this.Font), this.Font, bounds, color, false, true, 2, HorizontalAlignment.Center);
        }

        protected override string GetStatusTooltip() {
            return this.Player.Status.ToString();
        }
    }
}
