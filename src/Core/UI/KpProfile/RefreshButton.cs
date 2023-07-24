using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Nekres.ProofLogix.Core.UI.KpProfile {
    public class RefreshButton : Control {

        private DateTime _nextRefresh;
        public DateTime NextRefresh {
            get => _nextRefresh;
            set => SetProperty(ref _nextRefresh, value);
        }

        private AsyncTexture2D _tex;
        private AsyncTexture2D _hoverTex;
        private AsyncTexture2D _blockedTex;
        private bool           _isHovering;

        public RefreshButton() {
            _nextRefresh = DateTime.UtcNow;
            _tex        = GameService.Content.DatAssetCache.GetTextureFromAssetId(784346);
            _hoverTex   = GameService.Content.DatAssetCache.GetTextureFromAssetId(156330);
            _blockedTex = GameService.Content.DatAssetCache.GetTextureFromAssetId(851256);
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            _isHovering = false;
            base.OnMouseMoved(e);
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            _isHovering           = true;
            this.BasicTooltipText = "Refresh";
            base.OnMouseMoved(e);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            var remainingTime = NextRefresh.Subtract(DateTime.UtcNow);
            if (remainingTime.Ticks > 0) {
                if (_isHovering) {
                    var minutes = remainingTime.TotalMinutes > 1 ? "minutes" : "minute";
                    var seconds = remainingTime.TotalSeconds > 1 ? "seconds" : "second";
                    var timeSuffix  = remainingTime.TotalMinutes > 0 ? minutes : seconds;
                    this.BasicTooltipText = $"Refresh\nNext refresh available in {remainingTime:m\\:ss} {timeSuffix}.";
                }
                spriteBatch.DrawOnCtrl(this, _blockedTex, bounds);
            } else {
                this.BasicTooltipText = "Refresh";
            }
            spriteBatch.DrawOnCtrl(this, _isHovering ? _hoverTex : _tex, bounds, 
                                   remainingTime.Ticks > 0 ? Color.White * 0.7f : Color.White);
        }
    }
}
