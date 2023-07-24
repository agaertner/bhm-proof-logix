using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using Image = Blish_HUD.Controls.Image;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;

namespace Nekres.ProofLogix.Core.UI {
    internal class ItemWithAmount : Image {

        private int   _amount;
        public  int Amount { 
            get => _amount; 
            set => SetProperty(ref _amount, value);
        }

        private BitmapFont _font = GameService.Content.DefaultFont14;
        public BitmapFont Font {
            get => _font;
            set => SetProperty(ref _font, value);
        }

        private readonly Color _amountColor = new(255, 237, 159);

        public ItemWithAmount(AsyncTexture2D icon) : base(icon) {
            /* NOOP */
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            base.Paint(spriteBatch, bounds);

            var text = this.Amount.ToString(); 
            var dest = new Rectangle(-Panel.RIGHT_PADDING, 0, bounds.Width, bounds.Height);
            spriteBatch.DrawStringOnCtrl(this, text, this.Font, dest,
                                         _amountColor, false, true, 2, 
                                         HorizontalAlignment.Right, VerticalAlignment.Top);
        }
    }
}
