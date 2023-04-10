using System;
using System.Linq;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class TitleConverter : JsonConverter<Title> {

    public override bool CanWrite => false;
    public override bool CanRead  => true;

    public override Title ReadJson(JsonReader reader, Type objectType, Title existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null) {
            return null;
        }

        var obj = JObject.Load(reader);
        var prop = obj.Properties().FirstOrDefault();

        if (prop == null) {
            return null;
        }

        var displayName = prop.Name;

        if (string.IsNullOrEmpty(displayName)) {
            return null;
        }

        var originStr = prop.Value.Value<string>();

        if (string.IsNullOrEmpty(originStr)) {
            return null;
        }

        return new Title {
            Name = displayName,
            Origin = Enum.TryParse<Title.TitleOrigin>(originStr, true, out var result) ? result : Title.TitleOrigin.Unknown
        };
    }

    public override void WriteJson(JsonWriter writer, Title value, JsonSerializer serializer) {
        var obj = new JObject {
            {
                value.Name, value.Origin.ToString().ToLower()
            }
        };
        obj.WriteTo(writer);
    }
}