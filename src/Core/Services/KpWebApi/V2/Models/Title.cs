using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TitleMode = Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models.Title.TitleMode;
namespace Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models {
    public class Title {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TitleMode Mode { get; set; }
    }
}
