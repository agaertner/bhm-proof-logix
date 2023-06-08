using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace Nekres.ProofLogix.Core.UI {
    public class LoadingView : View {

        private readonly string _text; 

        public LoadingView(string text) {
            _text = text;
        }

        protected override void Build(Container buildPanel) {

            var spinner = new LoadingSpinner {
                Parent = buildPanel,
                Width = 64,
                Height = 64,
                Left = (buildPanel.ContentRegion.Width - 64) / 2,
                Top = (buildPanel.ContentRegion.Height - 64) / 2
            };

            var text = new Label {
                Parent              = buildPanel,
                Width               = buildPanel.ContentRegion.Width,
                Height              = 30,
                Text                = _text,
                Top                 = spinner.Bottom + Panel.BOTTOM_PADDING,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            buildPanel.ContentResized += (_, e) => {
                spinner.Left = (e.CurrentRegion.Width  - spinner.Width)  / 2;
                spinner.Top  = (e.CurrentRegion.Height - spinner.Height) / 2;
                text.Width   = buildPanel.ContentRegion.Width;
                text.Top     = spinner.Bottom + Panel.BOTTOM_PADDING;
            };

            base.Build(buildPanel);
        }

    }
}
