using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models {

    // 20230409220443
    // https://killproof.me/api/kp/Nekres.1943?lang=en

    public class Profile {

        public static Profile Empty => new() {
            IsEmpty = true
        };

        public bool IsEmpty { get; private init; }

        [JsonProperty("account_name")]
        public string Name { get; set; }

        [JsonProperty("valid_api_key")]
        public bool ValidApiKey { get; set; }

        [JsonProperty("proof_url")]
        public string ProofUrl { get; set; }

        [JsonProperty("last_refresh")]
        public DateTime LastRefresh { get; set; }

        [JsonProperty("kpid")]
        public string Id { get; set; }

        [JsonProperty("original_uce")]
        public OriginalUce OriginalUce { get; set; }

        [JsonProperty("tokens")]
        public List<Token> Tokens { get; set; }

        [JsonProperty("killproofs")]
        public List<Token> Killproofs { get; set; }

        [JsonProperty("coffers")]
        public List<Token> Coffers { get; set; }

        [JsonProperty("titles")]
        public List<Title> Titles { get; set; }

        [JsonProperty("linked")]
        public List<Profile> Linked { get; set; }

        [JsonProperty("linked_totals")]
        public LinkedTotals LinkedTotals { get; set; }

        [JsonIgnore]
        public List<Raid> Clears { get; set; }
    }

    public class LinkedTotals {
        [JsonProperty("tokens")]
        public List<Token> Tokens { get; set; }

        [JsonProperty("killproofs")]
        public List<Token> Killproofs { get; set; }

        [JsonProperty("coffers")]
        public List<Token> Coffers { get; set; }

        [JsonProperty("titles")]
        public List<Title> Titles { get; set; }
    }
}
