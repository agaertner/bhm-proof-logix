using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models {

    // 20230411144237
    // https://killproof.me/api/opener?encounter=vale_guardian&region=eu

    public sealed class Opener {

        public static Opener Empty = new() {
            IsEmpty = true
        };

        public bool IsEmpty { get; private init; }

        public enum ServerRegion {

            EU,
            NA

        }

        public string Encounter { get; set; }

        [JsonProperty("region")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ServerRegion Region { get; set; }

        public List<Volunteer> Volunteers { get; set; }
    }

    public sealed class Volunteer {

        [JsonProperty("account_name")]
        public string AccountName { get; set; }

        [JsonProperty("updated")]
        public DateTime Updated { get; set; }
    }
}
