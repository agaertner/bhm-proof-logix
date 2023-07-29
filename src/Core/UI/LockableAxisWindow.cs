using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.ProofLogix.Core.UI {
    internal class LockableAxisWindow : StandardWindow {

        public LockableAxisWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion) { }

        public LockableAxisWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion) { }

        public LockableAxisWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Point windowSize) : base(background, windowRegion, contentRegion, windowSize) { }

        public LockableAxisWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion, Point windowSize) : base(background, windowRegion, contentRegion, windowSize) { }

        /// <summary>
        /// Override to remove the width and height limits set by <see cref="WindowBase2.HandleWindowResize"/> and
        /// unlocks unrestricted resizing to fit arbitrarily scaling children (eg. tables).
        /// </summary>
        protected override Point HandleWindowResize(Point newSize) {
            var clamp = base.HandleWindowResize(newSize);
            return new Point(_size.X, clamp.Y); // Disable width resizing by the user.
        }

    }
}
