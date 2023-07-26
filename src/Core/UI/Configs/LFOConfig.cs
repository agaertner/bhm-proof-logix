using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.UI.Configs {
    public class LfoConfig : ConfigBase {

        private Opener.ServerRegion _region;
        [JsonProperty("region")]
        public Opener.ServerRegion Region {
            get => _region;
            set {
                _region                             = value;
                this.SaveConfig(ProofLogix.Instance.LfoConfig);
            }
        }
    }
}
