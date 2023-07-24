using Newtonsoft.Json;
using System;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models {
    internal class SecondsUntilDateTimeConverter : JsonConverter<DateTime> {

        public override bool CanRead  => true;
        public override bool CanWrite => false;
        
        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer) {
            if (reader.ValueType != typeof(long) || reader.Value == null) {
                throw new JsonSerializationException("Failed to deserialize value to DateTime.");
            }
            return DateTime.UtcNow.AddSeconds((long)reader.Value);
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}
