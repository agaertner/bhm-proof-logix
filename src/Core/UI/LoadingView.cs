using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using System;

namespace Nekres.ProofLogix.Core.UI {
    public class LoadingView : View {

        private AsyncString _title;
        private AsyncString _subtitle;

        private Label _titleLbl;
        private Label _subTitleLbl;

        public LoadingView(AsyncString title = null, AsyncString subtitle = null) {
            _title            =  title ?? string.Empty;
            _subtitle         =  subtitle ?? string.Empty;

            _title.Changed += OnTitleChanged;
            _subtitle.Changed += OnSubtitleChanged;
        }

        protected override void Unload() {
            _title.Changed    -= OnTitleChanged;
            _subtitle.Changed -= OnSubtitleChanged;
            base.Unload();
        }

        private void OnTitleChanged(object sender, EventArgs e) {
            if (_titleLbl == null) {
                return;
            }
            _titleLbl.Text = _title;
        }

        private void OnSubtitleChanged(object sender, EventArgs e) {
            if (_subTitleLbl == null) {
                return;
            }
            _subTitleLbl.Text = _subtitle;
        }

        protected override void Build(Container buildPanel) {

            var spinner = new LoadingSpinner {
                Parent = buildPanel,
                Width = 64,
                Height = 64,
                Left = (buildPanel.ContentRegion.Width - 64) / 2,
                Top = (buildPanel.ContentRegion.Height - 64) / 2
            };

            _titleLbl = new Label {
                Parent              = buildPanel,
                Width               = buildPanel.ContentRegion.Width,
                Height              = 30,
                Text                = _title,
                Top                 = spinner.Bottom + Panel.BOTTOM_PADDING,
                HorizontalAlignment = HorizontalAlignment.Center,
                Font = GameService.Content.DefaultFont18
            };

            _subTitleLbl = new Label {
                Parent              = buildPanel,
                Width               = buildPanel.ContentRegion.Width,
                Height              = 30,
                Text                = _subtitle,
                Top                 = _titleLbl.Bottom + Control.ControlStandard.ControlOffset.Y,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            buildPanel.ContentResized += (_, e) => {
                spinner.Left       = (e.CurrentRegion.Width  - spinner.Width)  / 2;
                spinner.Top        = (e.CurrentRegion.Height - spinner.Height) / 2;
                _titleLbl.Width    = buildPanel.ContentRegion.Width;
                _titleLbl.Top      = spinner.Bottom + Panel.BOTTOM_PADDING;
                _subTitleLbl.Width = buildPanel.ContentRegion.Width;
                _subTitleLbl.Top   = _titleLbl.Bottom + Control.ControlStandard.ControlOffset.Y;
            };

            base.Build(buildPanel);
        }
    }
}
