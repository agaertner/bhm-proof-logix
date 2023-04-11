using Newtonsoft.Json;
using System;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models {
    public class OriginalUce {

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("at_date")]
        public DateTime AtDate { get; set; }
    }
}
