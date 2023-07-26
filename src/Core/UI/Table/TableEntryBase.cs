using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.ProofLogix.Core.UI.Table {
    public abstract class TableEntryBase : Control {

        public event EventHandler<ValueEventArgs<int>> ColumnClick;
        
        private BitmapFont _font = GameService.Content.DefaultFont16;
        public BitmapFont Font {
            get => _font;
            set => SetProperty(ref _font, value);
        }

        private int _maxTimestampCellWidth = 150;
        public int MaxTimestampCellWidth {
            get => _maxTimestampCellWidth;
            set => SetProperty(ref _maxTimestampCellWidth, value);
        }

        private int _maxClassIconCellWidth = 32;
        public int MaxClassIconCellWidth {
            get => _maxClassIconCellWidth;
            set => SetProperty(ref _maxClassIconCellWidth, value);
        }

        private int _maxCharacterNameCellWidth = 150;
        public int MaxCharacterNameCellWidth {
            get => _maxCharacterNameCellWidth; 
            set => SetProperty(ref _maxCharacterNameCellWidth, value);
        }

        private int _maxAccountNameCellWidth = 150;
        public int MaxAccountNameCellWidth {
            get => _maxAccountNameCellWidth;
            set => SetProperty(ref _maxAccountNameCellWidth, value);
        }

        protected abstract string         Timestamp     { get; }
        protected abstract AsyncTexture2D ClassIcon     { get; }
        protected abstract string         CharacterName { get; }
        protected abstract string         AccountName   { get; }
        protected          bool           IsHovering    { get; private set; }

        private Rectangle       _timestampBounds;
        private Rectangle       _classIconBounds;
        private Rectangle       _characterNameBounds;
        private Rectangle       _accountNameBounds;
        private List<Rectangle> _tokenBounds;

        protected TableEntryBase() {
            /* NOOP */
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            this.BasicTooltipText = string.Empty;
            IsHovering            = false;
            base.OnMouseLeft(e);
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            IsHovering = true;
            base.OnMouseEntered(e);
        }

        protected override void OnClick(MouseEventArgs e) {
            if (_timestampBounds.Contains(this.RelativeMousePosition)) {
                ColumnClick?.Invoke(this, new ValueEventArgs<int>(0));
                return;
            }

            if (_classIconBounds.Contains(this.RelativeMousePosition)) {
                ColumnClick?.Invoke(this, new ValueEventArgs<int>(1));
                return;
            }

            if (_characterNameBounds.Contains(this.RelativeMousePosition)) {
                ColumnClick?.Invoke(this, new ValueEventArgs<int>(2));
                return;
            }

            if (_accountNameBounds.Contains(this.RelativeMousePosition)) {
                ColumnClick?.Invoke(this, new ValueEventArgs<int>(3));
                return;
            }

            var i = 4;
            foreach (var tokenBound in _tokenBounds) {
                if (tokenBound.Contains(this.RelativeMousePosition)) {
                    ColumnClick?.Invoke(this, new ValueEventArgs<int>(i));
                    return;
                }
                i++;
            }
            base.OnClick(e);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {

            var timestamp = Cut(this.Timestamp, this.MaxTimestampCellWidth);
            _timestampBounds = new Rectangle(0, 0, this.MaxTimestampCellWidth, bounds.Height);
            spriteBatch.DrawStringOnCtrl(this, timestamp, this.Font, _timestampBounds, Color.White, false, true, 2);

            _classIconBounds = new Rectangle(_timestampBounds.Right + ControlStandard.ControlOffset.X, 0, 32, 32);
            spriteBatch.DrawOnCtrl(this, this.ClassIcon, _classIconBounds);

            var characterName       = Cut(this.CharacterName, this.MaxCharacterNameCellWidth);
            _characterNameBounds = new Rectangle(_classIconBounds.Right + ControlStandard.ControlOffset.X, 0, this.MaxCharacterNameCellWidth, bounds.Height);
            spriteBatch.DrawStringOnCtrl(this, characterName, this.Font, _characterNameBounds, Color.White, false, true, 2);

            UpdateTooltip(_characterNameBounds, string.Empty);

            var accountName = Cut(this.AccountName, this.MaxAccountNameCellWidth);
            _accountNameBounds = new Rectangle(_characterNameBounds.Right + ControlStandard.ControlOffset.X, 0, this.MaxAccountNameCellWidth, bounds.Height);
            spriteBatch.DrawStringOnCtrl(this, accountName, this.Font, _accountNameBounds, Color.White, false, true, 2);

            UpdateTooltip(_accountNameBounds, string.Empty);

            var tempTokenBounds = new List<Rectangle>();
            var tokenBounds = _accountNameBounds;
            foreach (var obj in this.GetTokens(ProofLogix.Instance.TableConfig.Value.TokenIds.ToList())) {
                tokenBounds = new Rectangle(tokenBounds.Right + ControlStandard.ControlOffset.X * 3, 0, 30, bounds.Height);
                tempTokenBounds.Add(tokenBounds);
                PaintToken(spriteBatch, tokenBounds, obj);
            }
            _tokenBounds = tempTokenBounds;
        }

        private string Cut(string text, int maxWidth) {
            var result = text;
            var width  = (int)this.Font.MeasureString(result).Width;
            while (width > maxWidth) {
                result = result.Substring(0, result.Length - 1);
                width  = (int)this.Font.MeasureString(result).Width;
            }
            return result;
        }

        protected abstract IEnumerable<object> GetTokens(List<int> ids);

        protected abstract void PaintToken(SpriteBatch spriteBatch, Rectangle bounds, object obj);

        protected void UpdateTooltip(Rectangle bounds, string basicTooltipText) {
            if (!bounds.Contains(this.RelativeMousePosition)) {
                return;
            }
            this.BasicTooltipText = basicTooltipText;
        }
    }
}
