using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models.Converter;
using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models {

    [JsonConverter(typeof(TokenConverter))]
    public sealed class Token
    {
        public string Name { get; set; }

        public int Quantity { get; set; }
    }
}
