using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Control = Blish_HUD.Controls.Control;

namespace Nekres.ProofLogix.Core.UI {
    internal class ContextMenuStripItemWithColor : ContextMenuStripItem {

        private Color _textColor = Control.StandardColors.Default;
        public Color TextColor {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        private readonly        AsyncTexture2D _textureBullet = AsyncTexture2D.FromAssetId(155038);
        private static readonly Texture2D      _textureArrow  = Control.Content.GetTexture("context-menu-strip-submenu");

        public ContextMenuStripItemWithColor(string text) : base(text) {
            /* NOOP */
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            var color = this.Enabled ? (this.MouseOver ? Control.StandardColors.Tinted : Control.StandardColors.Default) : Control.StandardColors.DisabledText;

            if (this.CanCheck) {
                string state = this.Checked ? "-checked" : "-unchecked";
                string extension = "";
                extension = this.MouseOver ? "-active" : extension;
                extension = !this.Enabled ? "-disabled" : extension;
                spriteBatch.DrawOnCtrl(this, Checkable.TextureRegionsCheckbox.First(cb => cb.Name == "checkbox/cb" + state + extension), new Rectangle(-1, this._size.Y / 2 - 16, 32, 32), Control.StandardColors.Default);
            } else {
                spriteBatch.DrawOnCtrl(this, (Texture2D)this._textureBullet, new Rectangle(6, this._size.Y / 2 - 9, 18, 18), color);
            }

            spriteBatch.DrawStringOnCtrl(this, this.Text, Control.Content.DefaultFont14, new Rectangle(31, 1, this._size.X - 30 - 6, this._size.Y), Control.StandardColors.Shadow);
            spriteBatch.DrawStringOnCtrl(this, this.Text, Control.Content.DefaultFont14, new Rectangle(30, 0, this._size.X - 30 - 6, this._size.Y), this._enabled ? this.TextColor : Control.StandardColors.DisabledText);

            if (this.Submenu == null) {
                return;
            }

            spriteBatch.DrawOnCtrl(this, _textureArrow, new Rectangle(this._size.X - 6 - _textureArrow.Width, this._size.Y / 2 - _textureArrow.Height / 2, _textureArrow.Width, _textureArrow.Height), color);
        }
    }
}
