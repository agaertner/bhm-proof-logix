using Blish_HUD;
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

        protected override void PaintToken(SpriteBatch spriteBatch, Rectangle bounds, int tokenId) {
            var token = this.Player.KpProfile.GetToken(tokenId);

            var color = ProofLogix.Instance.PartySync.GetTokenAmountColor(tokenId, token.Amount, ProofLogix.Instance.TableConfig.Value.ColorGradingMode);

            spriteBatch.DrawStringOnCtrl(this, token.Amount.ToString(), this.Font, bounds, color, false, true, 2, HorizontalAlignment.Center);
            UpdateTooltip(bounds, token.Name);
        }
    }
}
