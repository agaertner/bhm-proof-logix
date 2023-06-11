using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models {

    // Paths: /api/kp/{account_name_OR_kp_id}?lang={code}
    //        /api/character/{character_name}/kp?lang={code}

    public class Profile : Proofs {

        public static Profile Empty = new() {
            NotFound = true
        };

        public new bool IsEmpty => this.Totals.IsEmpty;

        [JsonIgnore]
        public bool NotFound { get; private init; }

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

        [JsonProperty("linked")]
        public List<Profile> Linked { get; set; }

        private Proofs _totals;
        [JsonProperty("linked_totals")]
        public Proofs Totals { 
            get => _totals ?? this; 
            set => _totals = value;
        }

        [JsonIgnore]
        public List<Clear> Clears { get; set; }

        public bool BelongsTo(string accountName) {
            if (this.Name.Equals(accountName, StringComparison.InvariantCultureIgnoreCase)) {
                return true;
            }
            return this.Linked?.Any(profile => profile.Name.Equals(accountName, StringComparison.InvariantCultureIgnoreCase)) ?? false;
        }
    }

    public class Proofs : BaseResponse {

        public bool IsEmpty => (!this.Tokens?.Any()     ?? true) &&
                               (!this.Killproofs?.Any() ?? true) &&
                               (!this.Coffers?.Any()    ?? true) &&
                               (!this.Titles?.Any()     ?? true);

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

        public IEnumerable<Token> GetTokens() {
            var tokens = Enumerable.Empty<Token>();

            if (Tokens != null) {
                tokens = tokens.Concat(Tokens);
            }

            if (Killproofs != null) {
                tokens = tokens.Concat(Killproofs);
            }

            if (Coffers != null) {
                tokens = tokens.Concat(Coffers);
            }

            return tokens;
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
