using System.Collections.Generic;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models.Converter;
using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models {

    // Paths: /api/clear/{account_name_OR_kp_id}
    //        /api/character/{character_name}/clear

    [JsonConverter(typeof(RaidConverter))]
    public sealed class Clear : BaseResponse {
        public string Name { get; set; }

        public List<Boss> Encounters { get; set; }
    }

    [JsonConverter(typeof(BossConverter))]
    public sealed class Boss {
        public string Name { get; set; }
        public bool Cleared { get; set; }
    }
}
