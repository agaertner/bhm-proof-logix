using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Extended;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using Color = Microsoft.Xna.Framework.Color;
using Image = Blish_HUD.Controls.Image;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;

namespace Nekres.ProofLogix.Core.UI {
    internal class ItemWithAmount : Image {

        private int _amount;
        public  int Amount { 
            get => _amount;
            set {
                if (SetProperty(ref _amount, value)) {
                    if (_amount > 0) {
                        this.Tint    = Color.White;
                        this.Opacity = 1f;
                    } else {
                        this.Tint    = Color.White * 0.5f;
                        this.Opacity = 0.5f;
                    }
                }
            }
        }

        private Color _borderColor;
        public Color BorderColor {
            get => _borderColor;
            set => SetProperty(ref _borderColor, value);
        }

        private BitmapFont _font = GameService.Content.DefaultFont16;
        public BitmapFont Font {
            get => _font;
            set => SetProperty(ref _font, value);
        }

        private readonly Color _amountColor = new(255, 247, 169);

        private SpriteBatchParameters _grayscale;
        private SpriteBatchParameters _defaultSpriteBatchParameters;

        private ItemWithAmount(AsyncTexture2D icon) : base(icon) {
            _defaultSpriteBatchParameters = this.SpriteBatchParameters;
            _grayscale = new SpriteBatchParameters {
                Effect = ProofLogix.Instance.ContentsManager.GetEffect<Effect>(@"effects\grayscale.mgfx")
            };
            this.SpriteBatchParameters = _grayscale;
        }

        public static ItemWithAmount Create(int id, int amount) {
            var resource  = ProofLogix.Instance.Resources.GetItem(id);
            var tooltip   = new Tooltip();
            var labelText = ' ' + AssetUtil.GetItemDisplayName(resource.Name, amount, false);
            var labelSize = LabelUtil.GetLabelSize(ContentService.FontSize.Size20, labelText, true);
            var label = new FormattedLabelBuilder().SetWidth(labelSize.X)
                                                   .SetHeight(labelSize.Y + 10)
                                                   .SetVerticalAlignment(VerticalAlignment.Top)
                                                   .CreatePart(labelText, o => {
                                                        o.SetPrefixImage(resource.Icon);
                                                        o.SetPrefixImageSize(new Point(32, 32));
                                                        o.SetFontSize(ContentService.FontSize.Size20);
                                                        o.SetTextColor(resource.Rarity.AsColor());
                                                    }).Build();
            label.Parent = tooltip;

            return new ItemWithAmount(resource.Icon) {
                Width       = 64,
                Height      = 64,
                Amount      = amount,
                Tooltip     = tooltip,
                BorderColor = resource.Rarity.AsColor()
            };
        }

        protected override void DisposeControl() {
            _grayscale.Effect?.Dispose();
            base.DisposeControl();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {

            if (_grayscale.Effect != null) { 
                _grayscale.Effect.Parameters["Intensity"].SetValue(Convert.ToSingle(this.Amount <= 0));
                _grayscale.Effect.Parameters["Opacity"].SetValue(this.Opacity);
            }

            spriteBatch.End();
            spriteBatch.Begin(_grayscale);
            base.Paint(spriteBatch, bounds);
            spriteBatch.End();
            spriteBatch.Begin(_defaultSpriteBatchParameters); // Exclude everything below from grayscale effect.

            if (this.Amount > 0) {
                // Draw rarity border
                spriteBatch.DrawRectangleOnCtrl(this, bounds, 2, this.BorderColor);
            }

            if (this.Amount > 1) {
                // Draw quantity number
                var text = this.Amount.ToString();
                var dest = new Rectangle(-6, 2, bounds.Width, bounds.Height);
                spriteBatch.DrawStringOnCtrl(this, text, this.Font, dest,
                                             _amountColor, false, true, 2,
                                             HorizontalAlignment.Right, VerticalAlignment.Top);
            }
        }
    }
}
