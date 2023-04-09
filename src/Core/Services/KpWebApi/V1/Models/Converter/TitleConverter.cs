using System;
using System.Linq;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class TitleConverter : JsonConverter<Title> {
    public override Title ReadJson(JsonReader reader, Type objectType, Title existingValue, bool hasExistingValue, JsonSerializer serializer) {
        var obj         = JObject.Load(reader);
        var displayName = obj.Properties().FirstOrDefault()?.Name;

        if (string.IsNullOrEmpty(displayName)) {
            return null;
        }

        var originStr   = (string)obj[displayName];

        return new Title {
            DisplayName = displayName,
            Origin      = Enum.TryParse<Title.TitleOrigin>(originStr, true, out var result) ? result : Title.TitleOrigin.Unknown
        };
    }

    public override void WriteJson(JsonWriter writer, Title value, JsonSerializer serializer) {
        var obj = new JObject {
            {
                value.DisplayName, value.Origin.ToString().ToLower()
            }
        };
        obj.WriteTo(writer);
    }
}