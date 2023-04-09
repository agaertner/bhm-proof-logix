﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models {

    // 20230409220443
    // https://killproof.me/api/kp/Nekres.1943

    public class Account {

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
        public List<Token> Tokens { get; set; }

        [JsonProperty("killproofs")]
        public List<Token> Killproofs { get; set; }

        [JsonProperty("titles")]
        public List<Token> Titles { get; set; }
    }
}
