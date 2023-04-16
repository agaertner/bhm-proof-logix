using System.Net;
using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models {

    // Path: /proof/{account_name_OR_kp_id}/refresh

    public sealed class Refresh {

        [JsonProperty("status")]
        [JsonConverter(typeof(HttpStatusCodeConverter))]
        public HttpStatusCode Status { get; set; }

        [JsonProperty("min")]
        public int Minutes { get; set; }
    }
}
