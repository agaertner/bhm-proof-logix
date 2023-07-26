using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using Nekres.ProofLogix.Core.Services.PartySync.Models;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.ProofLogix.Core.UI.Table {
    public class TablePlayerEntry : TableEntryBase {

        private Player _player;
        public Player Player {
            get => _player;
            set => SetProperty(ref _player, value);
        }

        public TablePlayerEntry(Player player) : base() {
            _player = player;
        }

        protected override string         Timestamp     => this.Player.Created.ToLocalTime().AsTimeAgo();
        protected override AsyncTexture2D ClassIcon     => this.Player.Icon;
        protected override string         CharacterName => this.Player.CharacterName;
        protected override string         AccountName   => this.Player.AccountName;

        protected override IEnumerable<object> GetTokens(List<int> ids) {
            if (!this.Player.HasKpProfile) {
                return Enumerable.Empty<object>().ToList();
            }
            return ids.Select(this.Player.KpProfile.GetToken);
        }

        protected override void PaintToken(SpriteBatch spriteBatch, Rectangle bounds, object obj) {
            var token = (Token)obj;
            spriteBatch.DrawStringOnCtrl(this, token.Amount.ToString(), this.Font, bounds, Color.White, false, true, 2, HorizontalAlignment.Center);
            UpdateTooltip(bounds, token.Name);
        }
    }
}
