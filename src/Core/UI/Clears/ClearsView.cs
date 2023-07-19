using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework.Graphics;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Nekres.ProofLogix.Core.UI.Clears {
    public sealed class ClearsView : View {

        private readonly IReadOnlyList<Clear> _clears;

        private readonly Texture2D _greenTick;
        private readonly Texture2D _redCross;

        private ClearsView() {
            _greenTick = ProofLogix.Instance.ContentsManager.GetTexture("green-tick.gif");
            _redCross = ProofLogix.Instance.ContentsManager.GetTexture("red-cross.gif");
        }

        public ClearsView(List<Clear> clears) : this() {
            _clears = clears;
        }

        public ClearsView(List<string> clears) : this() {
            var tempClears = new List<Clear>();

            var raids = ProofLogix.Instance.Resources.GetRaids();

            foreach (var wing in raids.SelectMany(raid => raid.Wings)) {

                if (string.IsNullOrEmpty(wing.Id)) {
                    continue;
                }
                
                var clear = new Clear {
                    Name = wing.Name,
                    Encounters = new List<Boss>()
                };

                foreach (var ev in wing.Events) {
                    clear.Encounters.Add(new Boss {
                        Name    = ev.Name,
                        Cleared = clears.Any(id => id.Equals(ev.Id))
                    });
                }
                tempClears.Add(clear);
            }

            _clears = tempClears;
        }

        protected override void Unload() {
            _greenTick.Dispose();
            _redCross.Dispose();
            base.Unload();
        }

        protected override void Build(Container buildPanel) {
            var panel = new FlowPanel {
                Parent              = buildPanel,
                Width               = buildPanel.ContentRegion.Width,
                Height              = buildPanel.ContentRegion.Height,
                CanScroll           = true,
                FlowDirection       = ControlFlowDirection.SingleTopToBottom,
                OuterControlPadding = new Vector2(Panel.LEFT_PADDING, Panel.TOP_PADDING)
            };

            buildPanel.ContentResized += (_, e) => {
                panel.Width = e.CurrentRegion.Width;
                panel.Height = e.CurrentRegion.Height;
            };

            foreach (var clear in _clears) {

                var wingCategory = new FlowPanel {
                    Parent = panel,
                    Width = panel.ContentRegion.Width,
                    HeightSizingMode = SizingMode.AutoSize,
                    Title = clear.Name,
                    CanCollapse = true,
                    FlowDirection = ControlFlowDirection.SingleTopToBottom
                };

                panel.ContentResized += (_, e) => {
                    wingCategory.Width = e.CurrentRegion.Width;
                };

                foreach (var encounter in clear.Encounters) {

                    var icon = encounter.Cleared ? _greenTick : _redCross;
                    var size = LabelUtil.GetLabelSize(ContentService.FontSize.Size16, encounter.Name, true);

                    var encounterItem = new FormattedLabelBuilder()
                                       .SetWidth(size.X)
                                       .SetHeight(size.Y + Control.ControlStandard.ControlOffset.Y)
                                       .CreatePart(encounter.Name, o => {
                                           o.SetFontSize(ContentService.FontSize.Size16);
                                           o.SetPrefixImage(icon);
                                       }).Build();

                    encounterItem.Parent = wingCategory;
                }
            }
            base.Build(buildPanel);
        }
    }
}
