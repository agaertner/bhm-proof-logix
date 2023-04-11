using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models {

    [JsonConverter(typeof(TitleConverter))]
    public class Title {
        public enum TitleMode {
            Unknown,
            Raid,
            Fractal
        }

        public string Name { get; set; }

        public TitleMode Mode { get; set; }

    }
}
