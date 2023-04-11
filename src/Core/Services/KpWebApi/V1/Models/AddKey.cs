using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models {
    public class AddKey {

        [JsonProperty("kp_id")]
        public string KpId { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
