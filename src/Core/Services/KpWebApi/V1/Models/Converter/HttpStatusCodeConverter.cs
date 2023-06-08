using Newtonsoft.Json;
using System;
using System.Net;

public class HttpStatusCodeConverter : JsonConverter<HttpStatusCode> {

    public override HttpStatusCode ReadJson(JsonReader reader, Type objectType, HttpStatusCode existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.ValueType != typeof(int) && reader.ValueType != typeof(string) || reader.Value == null) {
            throw new JsonSerializationException($"Failed to deserialize value to HttpStatusCode.");
        }

        return Enum.TryParse(reader.Value.ToString().ToLower(), out HttpStatusCode result) ? result : HttpStatusCode.Accepted;
    }

    public override void WriteJson(JsonWriter writer, HttpStatusCode value, JsonSerializer serializer) {
        writer.WriteValue(value.ToString().ToLower());
    }
}