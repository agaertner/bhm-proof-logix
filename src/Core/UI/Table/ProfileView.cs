using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;

namespace Nekres.ProofLogix.Core.UI.Table {
    public class ProfileView : View {

        private readonly Profile _profile;

        public ProfileView(Profile profile) {
            _profile = profile;
        }

        protected override void Build(Container buildPanel) {

            var size   = LabelUtil.GetLabelSize(ContentService.FontSize.Size32, _profile.Name);

            var header = new FlowPanel {
                Parent              = buildPanel,
                Width               = buildPanel.ContentRegion.Width,
                Height              = 100,
                ControlPadding      = new Vector2(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                ShowBorder = true
            };

            var itemContainer = new FlowPanel {
                Parent              = buildPanel,
                Top                 = header.Bottom + Panel.TOP_PADDING,
                Width               = buildPanel.ContentRegion.Width,
                Height              = buildPanel.ContentRegion.Height - header.Height - Panel.TOP_PADDING,
                ControlPadding      = new Vector2(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                ShowBorder          = true
            };

            buildPanel.ContentResized += (_, e) => {
                header.Width         = e.CurrentRegion.Width;
                itemContainer.Width  = e.CurrentRegion.Width;
                itemContainer.Height = e.CurrentRegion.Height - header.Height - Panel.TOP_PADDING;
            };

            base.Build(buildPanel);
        }

    }
}
