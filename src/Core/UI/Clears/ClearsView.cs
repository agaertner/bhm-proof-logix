using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.ProofLogix.Core.UI.Clears {
    public sealed class ClearsView : View {

        private readonly IReadOnlyList<Clear> _clears;

        private readonly Texture2D _greenTick;
        private readonly Texture2D _redCross;

        private ClearsView() {
            _greenTick = ProofLogix.Instance.ContentsManager.GetTexture("green-tick.gif");
            _redCross = ProofLogix.Instance.ContentsManager.GetTexture("red-cross.gif");
        }

        /// <summary>
        /// Display encounter weekly clear states.
        /// </summary>
        /// <param name="clears">Clear state of encounters</param>
        public ClearsView(List<Clear> clears) : this() {
            _clears = clears;
        }

        /// <summary>
        /// Display encounter weekly clear states from <see cref="Services.Gw2WebApiService"/> like<br/>
        /// clears from <see cref="Services.KpWebApiService"/>.
        /// </summary>
        /// <param name="clears">Cleared encounter ids like they are reported by <see cref="Services.Gw2WebApiService.GetClears"/></param>
        public ClearsView(List<string> clears) : this() {
            var raids = ProofLogix.Instance.Resources.GetRaids();
            _clears = (from wing in raids.SelectMany(raid => raid.Wings)
                       where !string.IsNullOrEmpty(wing.Id)
                       select new Clear {
                           Name = ProofLogix.Instance.Resources.GetMapName(wing.MapId),
                           Encounters = wing.Events.Select(ev => new Boss {
                                                 Name    = ev.Name,
                                                 Cleared = clears.Any(id => id.Equals(ev.Id))
                                             })
                                            .ToList()
                       }).ToList();
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
                    Parent              = panel,
                    Width               = panel.ContentRegion.Width - 24,
                    HeightSizingMode    = SizingMode.AutoSize,
                    Title               = clear.Name,
                    CanCollapse         = true,
                    ControlPadding      = new Vector2(5, 5),
                    OuterControlPadding = new Vector2(5, 5),
                    FlowDirection       = ControlFlowDirection.SingleTopToBottom
                };

                panel.ContentResized += (_, e) => {
                    wingCategory.Width = e.CurrentRegion.Width - 24;
                };

                foreach (var encounter in clear.Encounters) {

                    var icon = encounter.Cleared ? _greenTick : _redCross;
                    var size = LabelUtil.GetLabelSize(ContentService.FontSize.Size20, encounter.Name, true);

                    var encounterItem = new FormattedLabelBuilder()
                                       .SetWidth(size.X)
                                       .SetHeight(size.Y + Control.ControlStandard.ControlOffset.Y)
                                       .CreatePart(encounter.Name, o => {
                                           o.SetFontSize(ContentService.FontSize.Size20);
                                           o.SetPrefixImage(icon);
                                           o.SetHyperLink(AssetUtil.GetWikiLink(encounter.Name));
                                       }).Build();

                    encounterItem.Parent = wingCategory;
                }
            }
            base.Build(buildPanel);
        }
    }
}
