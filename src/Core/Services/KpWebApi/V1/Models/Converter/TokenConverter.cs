using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models.Converter {
    public class TokenConverter : JsonConverter<Token> {
        public override bool CanWrite => false;

        public override Token ReadJson(JsonReader reader, Type objectType, Token existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var obj = JObject.Load(reader);
            var displayName = obj.Properties().FirstOrDefault()?.Name;

            if (string.IsNullOrEmpty(displayName)) {
                return null;
            }

            var quantity = (int)obj[displayName];

            return new Token {
                DisplayName = displayName,
                Quantity = quantity
            };
        }

        public override void WriteJson(JsonWriter writer, Token value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}