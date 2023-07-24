using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models {

    // Path: /proof/{account_name_OR_kp_id}/refresh

    public sealed class Refresh {

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("min")]
        public int Minutes { get; set; }
    }

    public sealed class ProofBusy {

        [JsonProperty("busy")]
        public int Busy { get; set; }

    }
}
