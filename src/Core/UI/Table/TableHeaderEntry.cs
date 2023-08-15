using Blish_HUD;
using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.ProofLogix.Core.UI.Table {
    internal class TableHeaderEntry : TableEntryBase {

        public TableHeaderEntry() : base() {
            /* NOOP */
        }

        private const    string         TIMESTAMP_TITLE = "#";
        private readonly AsyncTexture2D _classIcon      = GameService.Content.DatAssetCache.GetTextureFromAssetId(517179); // alternative 157085
        private const    string         CHAR_TITLE      = "Character";
        private const    string         ACCOUNT_TITLE   = "Account";

        protected override string         Timestamp     => TIMESTAMP_TITLE;
        protected override AsyncTexture2D ClassIcon     => _classIcon;
        protected override string         CharacterName => CHAR_TITLE;
        protected override string         AccountName   => ACCOUNT_TITLE;

        protected override void PaintToken(SpriteBatch spriteBatch, Rectangle bounds, int tokenId) {
            // Keep aspect ratio and center.
            var centered = new Rectangle(bounds.X + (bounds.Width - bounds.Height) / 2, bounds.Y, bounds.Height, bounds.Height);
            spriteBatch.DrawOnCtrl(this, ProofLogix.Instance.Resources.GetApiIcon(tokenId).Result, centered);
        }
    }
}
