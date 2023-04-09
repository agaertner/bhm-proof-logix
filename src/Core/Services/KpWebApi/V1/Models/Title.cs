using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models {

    [JsonConverter(typeof(TitleConverter))]
    public class Title {
        public enum TitleOrigin {
            Unknown,
            Raid,
            Fractal
        }

        public string Name { get; set; }

        public TitleOrigin Origin { get; set; }

    }
}
