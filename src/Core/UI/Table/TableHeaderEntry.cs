using Blish_HUD;
using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.ProofLogix.Core.UI.Table {
    internal class TableHeaderEntry : TableEntryBase {

        public TableHeaderEntry() : base() {
            /* NOOP */
        }

        private const      string         TIMESTAMP_TITLE = "#";
        private readonly   AsyncTexture2D _classIcon      = GameService.Content.DatAssetCache.GetTextureFromAssetId(517179); // alternative 157085
        private const      string         CHAR_TITLE      = "Character";
        private const      string         ACCOUNT_TITLE   = "Account";

        protected override string         Timestamp     => TIMESTAMP_TITLE;
        protected override AsyncTexture2D ClassIcon     => _classIcon;
        protected override string         CharacterName => CHAR_TITLE;
        protected override string         AccountName   => ACCOUNT_TITLE;

        protected override IEnumerable<object> GetTokens(List<int> ids) {
            return ids.Select(ProofLogix.Instance.Resources.GetApiIcon);
        }

        protected override void PaintToken(SpriteBatch spriteBatch, Rectangle bounds, object obj) {
            var icon = (AsyncTexture2D)obj;
            spriteBatch.DrawOnCtrl(this, icon, bounds);
        }
    }
}
