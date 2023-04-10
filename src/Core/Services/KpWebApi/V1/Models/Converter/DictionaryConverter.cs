using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models.Converter {
    public class DictionaryConverter<T> : JsonConverter {

        public override bool CanWrite => false;
        public override bool CanRead  => true;

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Dictionary<string, T>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Null) {
                return null;
            }

            var obj = JObject.Load(reader);

            var list = new List<T>();

            foreach (var property in obj.Properties()) {
                var item = new JObject { [property.Name] = property.Value };
                var value = item.ToObject<T>(serializer);
                list.Add(value);
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}
