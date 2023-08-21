using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.ProofLogix.Core.UI {
    public class FlowPanelWithIcon : FlowPanel {

        private AsyncTexture2D _icon;
        public AsyncTexture2D Icon {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        private Rectangle _layoutHeaderIconBounds;

        public FlowPanelWithIcon(AsyncTexture2D icon) {
            _icon = icon;
        }

        public override void RecalculateLayout() {
            base.RecalculateLayout(); // Recalculate private offsets first..

            // .. then use them in calculating icon bounds
            var layoutHeaderBounds = (Rectangle)this.GetPrivateField("_layoutHeaderBounds").GetValue(this);
            _layoutHeaderIconBounds = new Rectangle(layoutHeaderBounds.Left + 10, 2,
                                                    32, 32);

            // .. and recalculate title bounds to be to the right of the icon bounds.
            this.GetPrivateField("_layoutHeaderTextBounds")
                .SetValue(this, new Rectangle(_layoutHeaderIconBounds.Right + 7, 0, 
                                              layoutHeaderBounds.Width - _layoutHeaderIconBounds.Width - 10, 36));
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            base.PaintBeforeChildren(spriteBatch, bounds); // Draw header layout first.

            // Don't draw icon when no header layout was drawn eg. when there is no title.
            if (!string.IsNullOrEmpty(_title) && _icon is {HasTexture: true}) {
                spriteBatch.DrawOnCtrl(this, _icon, _layoutHeaderIconBounds, Color.White);
            }
        }

    }
}
