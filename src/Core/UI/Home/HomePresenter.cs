using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Nekres.ProofLogix.Core.Services.PartySync.Models;
using Nekres.ProofLogix.Core.UI.Table;

namespace Nekres.ProofLogix.Core.UI.Home {
    public class HomePresenter : Presenter<HomeView, object> {

        public HomePresenter(HomeView view, object model) : base(view, model) {
            ProofLogix.Instance.PartySync.PlayerAdded += OnPlayerAdded;
        }

        protected override void Unload() {
            ProofLogix.Instance.PartySync.PlayerAdded -= OnPlayerAdded;
            base.Unload();
        }

        private void OnPlayerAdded(object sender, ValueEventArgs<Player> e) {
            AddHistoryEntry(e.Value);
        }

        public void AddHistoryEntry(Player player) {
            var textSize = LabelUtil.GetLabelSize(ContentService.FontSize.Size18, player.AccountName, true);

            var label = new FormattedLabelBuilder().SetWidth(textSize.X)
                                                   .SetHeight(textSize.Y)
                                                   .CreatePart(player.AccountName, o => {
                                                        o.SetFontSize(ContentService.FontSize.Size18);
                                                        o.SetPrefixImage(player.Icon);
                                                        o.SetLink(() => ProfileView.Open(player.KpProfile));
                                                    })
                                                   .Build();

            label.Parent = this.View.HistoryPanel;
        }
    }
}
