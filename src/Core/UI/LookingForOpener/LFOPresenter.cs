using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;

namespace Nekres.ProofLogix.Core.UI.LookingForOpener {
    public class LfoPresenter : Presenter<LfoView, LfoConfig> {

        public LfoPresenter(LfoView view, LfoConfig model) : base(view, model) {

        }

        public async Task<Opener> GetOpener() {
            return await ProofLogix.Instance.KpWebApi.GetOpener(this.Model.EncounterId, this.Model.Region);
        }

        public void SetRegion(Opener.ServerRegion region) {
            this.Model.Region = region;
        }

        public void SetEncounterId(string encounterId) {
            this.Model.EncounterId = encounterId;
        }
    }
}
