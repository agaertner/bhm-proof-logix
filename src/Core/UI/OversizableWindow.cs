using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.ProofLogix.Core.UI {
    internal class OversizableWindow : StandardWindow {

        public OversizableWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion) { }

        public OversizableWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion) { }

        public OversizableWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Point windowSize) : base(background, windowRegion, contentRegion, windowSize) { }

        public OversizableWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion, Point windowSize) : base(background, windowRegion, contentRegion, windowSize) { }

        /// <summary>
        /// Override to remove the width and height limits set by <see cref="WindowBase2.HandleWindowResize"/> and
        /// unlocks unrestricted resizing to fit arbitrarily scaling children (eg. tables).
        /// </summary>
        protected override Point HandleWindowResize(Point newSize) {
            return new Point(_size.X, newSize.Y); // Disable width resizing by the user.
        }

    }
}
