using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models {

    // Paths: /api/kp/{account_name_OR_kp_id}
    //        /api/character/{character_name}/kp

    public class Profile {
        public static Profile Empty = new() {
            IsEmpty = true
        };

        [JsonIgnore]
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

        [JsonProperty("tokens")]
        [JsonConverter(typeof(DictionaryConverter<Token>))]
        public List<Token> Tokens { get; set; }

        [JsonProperty("killproofs")]
        [JsonConverter(typeof(DictionaryConverter<Token>))]
        public List<Token> Killproofs { get; set; }

        [JsonProperty("titles")]
        [JsonConverter(typeof(DictionaryConverter<Title>))]
        public List<Title> Titles { get; set; }

        [JsonIgnore]
        public List<Clear> Clears { get; set; }
    }

    [JsonConverter(typeof(TitleConverter))]
    public sealed class Title {
        public enum TitleMode {
            Unknown,
            Raid,
            Fractal
        }

        public string Name { get; set; }

        public TitleMode Mode { get; set; }

    }

    [JsonConverter(typeof(TokenConverter))]
    public sealed class Token {
        public string Name { get; set; }

        public int Quantity { get; set; }
    }
}
