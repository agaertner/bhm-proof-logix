using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models.Converter {
    public class RaidConverter : JsonConverter {

        public override bool CanRead  => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Raid);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var jObject = JObject.Load(reader);

            var firstPath   = jObject.First;

            if (firstPath == null) {
                return null;
            }

            var displayName = firstPath.Path;
            var bosses = firstPath.Values();

            var raid = new Raid {
                Name = displayName,
                Encounters = bosses.Select(boss =>
                                               serializer.Deserialize<Boss>(boss.CreateReader())
                                          ).ToList()
            };

            return raid;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}
