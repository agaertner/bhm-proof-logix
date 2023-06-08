using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models {

    // Path: /api/addkey

    public class AddKey : BaseResponse {

        [JsonProperty("kp_id")]
        public string KpId { get; set; }
    }
}
