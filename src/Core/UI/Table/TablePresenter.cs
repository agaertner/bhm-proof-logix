using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Nekres.ProofLogix.Core.Services;
using Nekres.ProofLogix.Core.Services.PartySync.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.UI.Table {
    public class TablePresenter : Presenter<TableView, TableConfig> {

        public TablePresenter(TableView view, TableConfig model) : base(view, model) {
            PartySyncService.OnPlayerAdded   += OnPlayerAddedOrChanged;
            PartySyncService.OnPlayerChanged += OnPlayerAddedOrChanged;
            PartySyncService.OnPlayerRemoved += OnPlayerRemoved;

        }

        protected override Task<bool> Load(IProgress<string> progress) {
            return base.Load(progress);
        }

        protected override void UpdateView() {

            base.UpdateView();
        }

        protected override void Unload() {
            PartySyncService.OnPlayerAdded   -= OnPlayerAddedOrChanged;
            PartySyncService.OnPlayerChanged -= OnPlayerAddedOrChanged;
            PartySyncService.OnPlayerRemoved -= OnPlayerRemoved;
            base.Unload();
        }

        private void OnPlayerRemoved(object sender, ValueEventArgs<Player> e) {
            var key = e.Value.AccountName?.ToLowerInvariant();

            if (string.IsNullOrEmpty(key)) {
                return;
            }

            this.View.Table.RemoveData(key);
        }

        private void OnPlayerAddedOrChanged(object sender, ValueEventArgs<Player> e) {
            var player = e.Value;
            var key = player.AccountName?.ToLowerInvariant();

            if (!player.HasKpProfile || string.IsNullOrEmpty(key)) {
                return;
            }

            var totals = player.KpProfile.LinkedTotals;

            this.View.Table.ChangeData(key, new object[] {
                player.Icon, player.CharacterName, player.AccountName, player.KpProfile?.Id ?? string.Empty,
                totals.Killproofs.First(x => x.Id == 77302).Amount, totals.Killproofs.First(x => x.Id == 94020).Amount
            });
        }
    }
}
