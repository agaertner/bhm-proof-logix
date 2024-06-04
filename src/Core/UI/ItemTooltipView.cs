using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Extended;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.UI {
    internal class ItemTooltipView : View {

        private int       _amount;
        private int       _itemId;
        private Texture2D _icon;
        private Item      _item;

        public ItemTooltipView(int itemId, int amount) {
            _itemId = itemId;
            _amount = amount;
        }

        protected override async Task<bool> Load(IProgress<string> progress) {
            progress.Report("Loading…");
            _item = (await ProofLogix.Instance.Gw2WebApi.GetItems(_itemId)).FirstOrDefault();
            if (_item != null) {
                _icon = GameService.Content.GetRenderServiceTexture(_item.Icon);
            }
            progress.Report(null);
            return _item != null;
        }

        protected override void Build(Container buildPanel) {
            var labelText = ' ' + AssetUtil.GetItemDisplayName(_item.Name, _amount, false);
            var labelSize = LabelUtil.GetLabelSize(ContentService.FontSize.Size20, labelText, true);
            var label = new FormattedLabelBuilder().SetWidth(labelSize.X  + 10)
                                                   .SetHeight(labelSize.Y + 10)
                                                   .SetVerticalAlignment(VerticalAlignment.Top)
                                                   .CreatePart(labelText, o => {
                                                        o.SetPrefixImage(_icon);
                                                        o.SetPrefixImageSize(new Point(32, 32));
                                                        o.SetFontSize(ContentService.FontSize.Size20);
                                                        o.SetTextColor(_item.Rarity.Value.AsColor());
                                                        o.MakeBold();
                                                    }).Build();
            label.Parent = buildPanel;
            base.Build(buildPanel);
        }
    }
}
