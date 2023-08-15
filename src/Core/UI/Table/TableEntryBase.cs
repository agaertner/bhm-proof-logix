using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Nekres.ProofLogix.Core.UI.Configs;
using System;
using System.Collections.Generic;

namespace Nekres.ProofLogix.Core.UI.Table {
    public abstract class TableEntryBase : Control {

        public event EventHandler<ValueEventArgs<int>> ColumnClick;
        
        private BitmapFont _font = GameService.Content.DefaultFont16;
        public BitmapFont Font {
            get => _font;
            set => SetProperty(ref _font, value);
        }

        private int _maxStatusIconCellWidth = 11;
        public int MaxStatusIconCellWidth {
            get => _maxStatusIconCellWidth;
            set => SetProperty(ref _maxStatusIconCellWidth, value);
        }

        private int _maxTimestampCellWidth = 150;
        public int MaxTimestampCellWidth {
            get => _maxTimestampCellWidth;
            set => SetProperty(ref _maxTimestampCellWidth, value);
        }

        private int _maxClassIconCellWidth = 36;
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

        private int _maxTokenCellWidth = 50;
        public int MaxTokenCellWidth {
            get => _maxTokenCellWidth;
            set => SetProperty(ref _maxTokenCellWidth, value);
        }

        protected abstract string         Timestamp     { get; }
        protected abstract AsyncTexture2D ClassIcon     { get; }
        protected abstract string         CharacterName { get; }
        protected abstract string         AccountName   { get; }
        protected          bool           IsHovering    { get; private set; }

        private Rectangle       _statusIconBounds;
        private Rectangle       _timestampBounds;
        private Rectangle       _classIconBounds;
        private Rectangle       _characterNameBounds;
        private Rectangle       _accountNameBounds;
        private List<Rectangle> _tokenBounds;

        protected TableEntryBase() {
            _timestampBounds = Rectangle.Empty;
            _classIconBounds = Rectangle.Empty;
            _characterNameBounds = Rectangle.Empty;
            _accountNameBounds = Rectangle.Empty;
            _tokenBounds = new List<Rectangle>();
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
            base.OnClick(e);

            if (_statusIconBounds.Contains(this.RelativeMousePosition)) {
                ColumnClick?.Invoke(this, new ValueEventArgs<int>((int)TableConfig.Column.Status));
                return;
            }

            if (_timestampBounds.Contains(this.RelativeMousePosition)) {
                ColumnClick?.Invoke(this, new ValueEventArgs<int>((int)TableConfig.Column.Timestamp));
                return;
            }

            if (_classIconBounds.Contains(this.RelativeMousePosition)) {
                ColumnClick?.Invoke(this, new ValueEventArgs<int>((int)TableConfig.Column.Class));
                return;
            }

            if (_characterNameBounds.Contains(this.RelativeMousePosition)) {
                ColumnClick?.Invoke(this, new ValueEventArgs<int>((int)TableConfig.Column.Character));
                return;
            }

            if (_accountNameBounds.Contains(this.RelativeMousePosition)) {
                ColumnClick?.Invoke(this, new ValueEventArgs<int>((int)TableConfig.Column.Account));
                return;
            }

            var i = Enum.GetValues(typeof(TableConfig.Column)).Length;
            foreach (var tokenBound in _tokenBounds) {
                if (tokenBound.Contains(this.RelativeMousePosition)) {
                    ColumnClick?.Invoke(this, new ValueEventArgs<int>(i));
                    return;
                }
                i++;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {

            var columns = ProofLogix.Instance.TableConfig.Value.Columns;

            // Status
            if (columns.Contains(TableConfig.Column.Status)) {
                _statusIconBounds = new Rectangle(ControlStandard.ControlOffset.X, 0, this.MaxStatusIconCellWidth, bounds.Height);
                PaintStatus(spriteBatch, _statusIconBounds);
                UpdateTooltip(_statusIconBounds, GetStatusTooltip());
            } else {
                _statusIconBounds = Rectangle.Empty; 
            }

            // Timestamp
            if (columns.Contains(TableConfig.Column.Timestamp)) {
                var timestamp = AssetUtil.Truncate(this.Timestamp, this.MaxTimestampCellWidth, this.Font);
                _timestampBounds = new Rectangle(_statusIconBounds.Right + ControlStandard.ControlOffset.X, 0, this.MaxTimestampCellWidth, bounds.Height);
                spriteBatch.DrawStringOnCtrl(this, timestamp, this.Font, _timestampBounds, Color.White, false, true, 2);
                UpdateTooltip(_timestampBounds, GetTimestampTooltip());
            } else {
                _timestampBounds = new Rectangle(_statusIconBounds.Right, 0, 0, 0);
            }

            // Class Icon
            if (columns.Contains(TableConfig.Column.Class)) {
                _classIconBounds = new Rectangle(_timestampBounds.Right + ControlStandard.ControlOffset.X, 0, this.MaxClassIconCellWidth, bounds.Height);
                // Keep aspect ratio and center.
                var centered = new Rectangle(_classIconBounds.X + (_classIconBounds.Width - _classIconBounds.Height) / 2, _classIconBounds.Y + (bounds.Height - _classIconBounds.Height) / 2, _classIconBounds.Height, _classIconBounds.Height);
                spriteBatch.DrawOnCtrl(this, this.ClassIcon, centered);
                UpdateTooltip(_classIconBounds, GetClassTooltip());
            } else {
                _classIconBounds = new Rectangle(_timestampBounds.Right, 0, 0, 0);
            }

            // Character Name
            if (columns.Contains(TableConfig.Column.Character)) {
                var characterName = AssetUtil.Truncate(this.CharacterName, this.MaxCharacterNameCellWidth, this.Font);
                _characterNameBounds = new Rectangle(_classIconBounds.Right + ControlStandard.ControlOffset.X, 0, this.MaxCharacterNameCellWidth, bounds.Height);
                spriteBatch.DrawStringOnCtrl(this, characterName, this.Font, _characterNameBounds, Color.White, false, true, 2);
                UpdateTooltip(_characterNameBounds, GetCharacterTooltip());
            } else {
                _characterNameBounds = new Rectangle(_classIconBounds.Right, 0, 0, 0);
            }

            // Account Name
            if (columns.Contains(TableConfig.Column.Account)) {
                var accountName = AssetUtil.Truncate(this.AccountName, this.MaxAccountNameCellWidth, this.Font);
                _accountNameBounds = new Rectangle(_characterNameBounds.Right + ControlStandard.ControlOffset.X, 0, this.MaxAccountNameCellWidth, bounds.Height);
                spriteBatch.DrawStringOnCtrl(this, accountName, this.Font, _accountNameBounds, Color.White, false, true, 2);
                UpdateTooltip(_accountNameBounds, GetAccountTooltip());
            } else {
                _accountNameBounds = new Rectangle(_characterNameBounds.Right, 0, 0, 0);
            }

            // Tokens (dynamic amount of trailing columns)
            var tempTokenBounds = new List<Rectangle>();
            var tokenBounds = _accountNameBounds;
            foreach (var id in ProofLogix.Instance.TableConfig.Value.TokenIds) {
                tokenBounds = new Rectangle(tokenBounds.Right + ControlStandard.ControlOffset.X * 3, 0, this.MaxTokenCellWidth, bounds.Height);
                tempTokenBounds.Add(tokenBounds);
                PaintToken(spriteBatch, tokenBounds, id);
                UpdateTooltip(tokenBounds, GetTokenTooltip(id));
            }
            _tokenBounds = tempTokenBounds;

            this.Width = tokenBounds.Right + ControlStandard.ControlOffset.X;
        }

        protected virtual string GetStatusTooltip() {
            return string.Empty;
        }

        protected virtual string GetTimestampTooltip() {
            return string.Empty;
        }

        protected virtual string GetClassTooltip() {
            return string.Empty;
        }

        protected virtual string GetCharacterTooltip() {
            return string.Empty;
        }

        protected virtual string GetAccountTooltip() {
            return string.Empty;
        }

        protected virtual string GetTokenTooltip(int tokenId) {
            return ProofLogix.Instance.Resources.GetItem(tokenId).Name;
        }

        protected abstract void PaintStatus(SpriteBatch spriteBatch, Rectangle bounds);

        protected abstract void PaintToken(SpriteBatch spriteBatch, Rectangle bounds, int tokenId);

        private void UpdateTooltip(Rectangle bounds, string basicTooltipText) {
            if (!bounds.Contains(this.RelativeMousePosition)) {
                return;
            }
            this.BasicTooltipText = basicTooltipText;
        }
    }
}
