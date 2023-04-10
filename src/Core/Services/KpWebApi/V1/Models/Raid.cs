using System.Collections.Generic;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models.Converter;
using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models {

    // 20230409214725
    // https://killproof.me/api/clear/Nekres.1943

    [JsonConverter(typeof(RaidConverter))]
    public class Raid {
        public string Name { get; set; }

        public List<Boss> Encounters { get; set; }
    }

    [JsonConverter(typeof(BossConverter))]
    public class Boss {
        public string Name { get; set; }
        public bool Cleared { get; set; }
    }
}
