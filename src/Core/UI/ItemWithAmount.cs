using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Extended;
using MonoGame.Extended.BitmapFonts;
using Color = Microsoft.Xna.Framework.Color;
using Image = Blish_HUD.Controls.Image;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;

namespace Nekres.ProofLogix.Core.UI {
    internal class ItemWithAmount : Image {

        private int   _amount;
        public  int Amount { 
            get => _amount; 
            set => SetProperty(ref _amount, value);
        }

        private Color _borderColor;
        public Color BorderColor {
            get => _borderColor;
            set => SetProperty(ref _borderColor, value);
        }

        private BitmapFont _font = GameService.Content.DefaultFont16;
        public BitmapFont Font {
            get => _font;
            set => SetProperty(ref _font, value);
        }

        private readonly Color _amountColor = new(255, 247, 169);

        public ItemWithAmount(AsyncTexture2D icon) : base(icon) {
            /* NOOP */
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            base.Paint(spriteBatch, bounds);

            // Draw rarity border
            spriteBatch.DrawRectangleOnCtrl(this, bounds, 2, this.BorderColor);

            if (this.Amount > 1) {
                // Draw quantity number
                var text = this.Amount.ToString();
                var dest = new Rectangle(-6, 2, bounds.Width, bounds.Height);
                spriteBatch.DrawStringOnCtrl(this, text, this.Font, dest,
                                             _amountColor, false, true, 2,
                                             HorizontalAlignment.Right, VerticalAlignment.Top);
            }
        }
    }
}
