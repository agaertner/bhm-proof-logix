using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models {

    // Paths: /api/kp/{account_name_OR_kp_id}?lang={code}
    //        /api/character/{character_name}/kp?lang={code}

    public class Profile : BaseResponse, IProfileV2 {

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
        public LinkedTotals LinkedTotals;

        [JsonIgnore]
        public List<Clear> Clears { get; set; }

        public Token GetToken(int id) {
            return Tokens?.FirstOrDefault(x => x.Id == id) ??
                   Killproofs?.FirstOrDefault(x => x.Id == id) ??
                   Coffers?.FirstOrDefault(x => x.Id == id);
        }
    }

    public sealed class LinkedTotals : IProfileV2 {
        [JsonProperty("tokens")]
        public List<Token> Tokens { get; set; }

        [JsonProperty("killproofs")]
        public List<Token> Killproofs { get; set; }

        [JsonProperty("coffers")]
        public List<Token> Coffers { get; set; }

        [JsonProperty("titles")]
        public List<Title> Titles { get; set; }

        public Token GetToken(int id) {
            return Tokens?.FirstOrDefault(x => x.Id == id) ??
                   Killproofs?.FirstOrDefault(x => x.Id == id) ??
                   Coffers?.FirstOrDefault(x => x.Id    == id);
        }
    }

    public sealed class OriginalUce {

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("at_date")]
        public DateTime AtDate { get; set; }
    }

    public sealed class Title {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public V1.Models.Title.TitleMode Mode { get; set; }
    }

    public sealed class Token {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }
    }
}
