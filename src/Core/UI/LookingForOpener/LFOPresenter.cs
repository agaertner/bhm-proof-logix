using Blish_HUD.Graphics.UI;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Nekres.ProofLogix.Core.UI.Configs;
using System;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.UI.LookingForOpener {
    public class LfoPresenter : Presenter<LfoView, LfoConfig> {

        public LfoPresenter(LfoView view, LfoConfig model) : base(view, model) {

        }

        public async Task<Opener> GetOpener(string encounterId) {
            return await ProofLogix.Instance.KpWebApi.GetOpener(encounterId, this.Model.Region);
        }

        public void SetRegion(string serverRegion) {
            if (!Enum.TryParse<Opener.ServerRegion>(serverRegion, true, out var region)) {
                return;
            }
            this.Model.Region = region;
        }
    }
}
