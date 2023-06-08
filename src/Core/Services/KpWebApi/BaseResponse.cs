using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.Services.KpWebApi {
    public class BaseResponse {
        [JsonProperty("error")]
        public string Error { get; set; }

        public bool IsError => !string.IsNullOrEmpty(this.Error);
    }
}
