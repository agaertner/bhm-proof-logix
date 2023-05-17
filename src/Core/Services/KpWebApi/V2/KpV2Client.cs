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

        public async Task<Profile> GetProfile(string id) {
            var profile = await HttpUtil.RetryAsync<Profile>(() => _uri.AppendPathSegments("kp", id)
                                                                       .SetQueryParam($"lang={GameService.Overlay.UserLocale.Value.Code()}").GetAsync());
            return profile ?? Profile.Empty;
        }

        public async Task<Profile> GetProfileByCharacterName(string characterName) {
            var profile = await HttpUtil.RetryAsync<Profile>(() => _uri.AppendPathSegments("character", characterName, "kp")
                                                                       .SetQueryParam($"lang={GameService.Overlay.UserLocale.Value.Code()}").GetAsync());
            return profile ?? Profile.Empty;
        }
    }
}
