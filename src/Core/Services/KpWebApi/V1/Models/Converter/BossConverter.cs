using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models.Converter {
    public class BossConverter : JsonConverter {

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Boss);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Null) {
                return null;
            }

            var jObject  = JObject.Load(reader);
            var property = jObject.Properties().First();

            return new Boss {
                Name = property.Name,
                Clears      = property.Value.Value<int>()
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}