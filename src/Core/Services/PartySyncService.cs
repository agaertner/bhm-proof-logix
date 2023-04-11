using Blish_HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.ArcDps.Common;

namespace Nekres.ProofLogix.Core.Services {
    internal class PartySyncService {

        public PartySyncService() {
            GameService.ArcDps.Common.Activate();

            GameService.ArcDps.Common.PlayerAdded   += OnPlayerAdded;
            GameService.ArcDps.Common.PlayerRemoved += OnPlayerRemoved;
        }

        private void OnPlayerAdded(CommonFields.Player player) {
            player.AccountName
        }

        private void OnPlayerRemoved(CommonFields.Player player) {
            throw new NotImplementedException();
        }


    }
}
