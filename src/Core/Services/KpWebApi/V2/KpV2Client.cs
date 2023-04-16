using Blish_HUD;
using Blish_HUD.Extended;
using Flurl;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using Nekres.ProofLogix.Core.Utils;
using System.Threading.Tasks;
using Flurl.Http;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V2 {
    internal class KpV2Client {

        private readonly string _uri = "https://killproof.me/api/";

        public async Task<Profile> GetProfile(string id, bool isCharacterName = false) {
            var profile = await HttpUtil.RetryAsync<Profile>(isCharacterName ?
                                                                 () => _uri.AppendPathSegments("character", id, "kp")
                                                                           .SetQueryParam($"lang={GameService.Overlay.UserLocale.Value.Code()}").GetAsync() : 
                                                                 () => _uri.AppendPathSegments("kp", id)
                                                                           .SetQueryParam($"lang={GameService.Overlay.UserLocale.Value.Code()}").GetAsync());
            return profile ?? Profile.Empty;
        }
    }
}
