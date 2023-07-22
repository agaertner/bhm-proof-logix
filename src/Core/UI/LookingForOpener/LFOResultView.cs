using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.ProofLogix.Core.Services;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using System.Linq;

namespace Nekres.ProofLogix.Core.UI.LookingForOpener {
    public class LfoResultView : View {

        private readonly LfoResults _results;

        public LfoResultView(LfoResults results) {
            _results = results;
        }

        protected override void Build(Container buildPanel) {

            var encounter = ProofLogix.Instance.Resources.GetWings().SelectMany(wing => wing.Events).First(ev => ev.Id.Equals(_results.EncounterId));

            var size = LabelUtil.GetLabelSize(ContentService.FontSize.Size32, encounter.Name, true);
            var header = new FormattedLabelBuilder()
                             .SetWidth(size.X)
                             .SetHeight(size.Y)
                             .CreatePart(encounter.Name, o => {
                                  o.SetFontSize(ContentService.FontSize.Size32);
                                  o.SetHyperLink($"https://wiki.guildwars2.com/wiki/{encounter.Name}");
                                  o.SetPrefixImage(encounter.Icon);
                              }).Build();

            header.Parent = buildPanel;
            header.Left   = Control.ControlStandard.ControlOffset.X;
            header.Top    = Control.ControlStandard.ControlOffset.Y;

            var flow = new FlowPanel {
                Parent              = buildPanel,
                Left                = Panel.LEFT_PADDING,
                Top                 = header.Bottom,
                Width               = buildPanel.ContentRegion.Width,
                Height              = buildPanel.ContentRegion.Height - header.Height - Panel.TOP_PADDING,
                ControlPadding      = new Vector2(Control.ControlStandard.ControlOffset.X, Control.ControlStandard.ControlOffset.Y),
                OuterControlPadding = new Vector2(Control.ControlStandard.ControlOffset.X, Control.ControlStandard.ControlOffset.Y),
                FlowDirection       = ControlFlowDirection.SingleTopToBottom,
                CanScroll           = true
            };

            buildPanel.ContentResized += (_,e) => {
                flow.Width  = e.CurrentRegion.Width;
                flow.Height = e.CurrentRegion.Height - header.Height - Panel.TOP_PADDING;
            };

            if (_results.Opener.IsEmpty) {
                var text      = "No volunteers found.";

                var fontSize  = ContentService.FontSize.Size24;
                var labelSize = LabelUtil.GetLabelSize(fontSize, text);
                var label = new FormattedLabelBuilder().SetHeight(labelSize.Y).SetWidth(labelSize.X)
                                                       .CreatePart(text, o => {
                                                            o.SetFontSize(fontSize);
                                                            o.SetPrefixImage(GameService.Content.GetTexture("common/1444522"));
                                                            o.SetTextColor(Color.Red);
                                                        }).Build();
                label.Parent = flow;

                base.Build(buildPanel);
                return;
            }

            foreach (var volunteer in _results.Opener.Volunteers) {
                var fontSize  = ContentService.FontSize.Size24;

                var labelSize = LabelUtil.GetLabelSize(fontSize, volunteer.AccountName + volunteer.Updated.AsTimeAgo());

                var label = new FormattedLabelBuilder().SetHeight(labelSize.Y).SetWidth(labelSize.X)
                                                       .CreatePart(volunteer.AccountName, o => {
                                                            o.SetLink(() => CopyText(volunteer.AccountName));
                                                        }).CreatePart(volunteer.Updated.AsTimeAgo(), o => {
                                                            o.SetFontSize(ContentService.FontSize.Size11);
                                                            o.MakeItalic();
                                                        }).Build();
                label.Parent = flow;
            }

            base.Build(buildPanel);
        }

        private async void CopyText(string text) {

            if (!await ClipboardUtil.WindowsClipboardService.SetTextAsync(text)) {
                return;
            }

            ScreenNotification.ShowNotification($"'{text}' copied to clipboard.");
        }

    }

    public class LfoResults {

        public string EncounterId { get; init; }
        public Opener Opener      { get; init; }

        public LfoResults(string encounterId, Opener openerTask) {
            this.EncounterId = encounterId;
            this.Opener      = openerTask;
        }
    }
}
