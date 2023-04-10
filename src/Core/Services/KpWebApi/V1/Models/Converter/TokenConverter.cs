using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models.Converter {
    public class TokenConverter : JsonConverter<Token> {

        public override bool CanWrite => false;
        public override bool CanRead  => true;

        public override Token ReadJson(JsonReader reader, Type objectType, Token existingValue, bool hasExistingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Null) {
                return null;
            }

            var obj  = JObject.Load(reader);
            var prop = obj.Properties().FirstOrDefault();

            if (prop == null) {
                return null;
            }

            var displayName = prop.Name;

            if (string.IsNullOrEmpty(displayName)) {
                return null;
            }

            var quantity = Convert.ToInt32(prop.Value.Value<int>());

            return new Token {
                Name     = displayName,
                Quantity = quantity
            };
        }


        public override void WriteJson(JsonWriter writer, Token value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}