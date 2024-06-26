﻿using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
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

        [JsonProperty("next_refresh")]
        public DateTime NextRefresh { get; set; }

        [JsonProperty("next_refresh_seconds")]
        [JsonConverter(typeof(SecondsUntilDateTimeConverter))]
        [Obsolete("Use NextRefresh instead.", true)]
        public DateTime NextRefreshFromSeconds { get; set; }

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

        #region Shorthands
        public List<Profile> Accounts => this.Linked?.Prepend(this).ToList() ?? new List<Profile> { this };

        public new bool IsEmpty  => this.Totals.IsEmpty;
        #endregion

        public bool BelongsTo(string accountName, out Profile linkedProfile) {
            linkedProfile = this.Accounts?.FirstOrDefault(profile => !string.IsNullOrEmpty(profile.Name) 
                                                                  && profile.Name.Equals(accountName, StringComparison.InvariantCultureIgnoreCase));
            return !(linkedProfile ?? Empty).NotFound;
        }
    }

    public class Proofs : BaseResponse {

        public bool IsEmpty => (!GetTokens().Any() || GetTokens().All(t => t.Amount == 0)) 
                            && (!Titles?.Any() ?? true);


        [JsonProperty("tokens")]
        public List<Token> Tokens { get; set; }

        [JsonProperty("killproofs")]
        public List<Token> Killproofs { get; set; }

        [JsonProperty("coffers")]
        public List<Token> Coffers { get; set; }

        [JsonProperty("titles")]
        public List<Title> Titles { get; set; }

        public virtual Token GetToken(int id) {
            return Killproofs?.FirstOrDefault(x => x.Id == id) ??
                   Tokens?.FirstOrDefault(x => x.Id == id) ??
                   Coffers?.FirstOrDefault(x => x.Id == id) ?? Token.Empty;
        }

        public Proofs() {
            Tokens = new List<Token>();
            Killproofs = new List<Token>();
            Coffers = new List<Token>();
            Titles = new List<Title>();
        }

        public IEnumerable<Token> GetTokens(bool excludeCoffers = false) {
            var tokens = Enumerable.Empty<Token>();

            if (Tokens != null) {
                tokens = tokens.Concat(Tokens);
            }

            if (Killproofs != null) {
                tokens = tokens.Concat(Killproofs);
            }

            if (Coffers != null && !excludeCoffers) {
                tokens = tokens.Concat(Coffers);
            }

            return tokens.GroupBy(token => token.Id).Select(group => group.First());
        }
    }

    public sealed class OriginalUce : Token {
        [JsonProperty("at_date")]
        public DateTime AtDate { get; set; }

        public OriginalUce() {
            this.Id   = Resources.UNSTABLE_COSMIC_ESSENCE;
            this.Name = ProofLogix.Instance.Resources.GetItem(Id).Name;
        }
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

    public class Token {

        public static Token Empty = new() {
            Name = string.Empty,
            IsEmpty = true
        };

        [JsonIgnore]
        public bool IsEmpty { get; private init; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }
    }
}
