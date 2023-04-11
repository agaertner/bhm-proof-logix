using System.Net;
using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models {

    // 20230411140415
    // https://killproof.me/proof/Nekres.1943/refresh

    public sealed class Refresh {

        [JsonProperty("status")]
        [JsonConverter(typeof(HttpStatusCodeConverter))]
        public HttpStatusCode Status { get; set; }

        [JsonProperty("min")]
        public int Minutes { get; set; }
    }
}
