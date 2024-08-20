using Blish_HUD;
using Blish_HUD.Extended;
using Flurl;
using Flurl.Http;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using System.Threading.Tasks;
namespace Nekres.ProofLogix.Core.Services.KpWebApi.V2 {
    internal class KpV2Client {

        private readonly string _uri = "https://killproof.me/api/";

        public async Task<Profile> GetProfile(string id) {
            var profile = await HttpUtil.RetryAsync<Profile>(() => _uri.AppendPathSegments("kp", id)
                                                                       .SetQueryParam($"lang={GameService.Overlay.UserLocale.Value.TwoLetterISOLanguageName()}")
                                                                       .GetAsync());
            return profile ?? Profile.Empty;
        }

        public async Task<Profile> GetProfileByCharacter(string characterName) {
            var profile = await HttpUtil.RetryAsync<Profile>(() => _uri.AppendPathSegments("character", characterName, "kp")
                                                                       .SetQueryParam($"lang={GameService.Overlay.UserLocale.Value.TwoLetterISOLanguageName()}")
                                                                       .GetAsync());
            return profile ?? Profile.Empty;
        }
            
        public async Task<Resources> GetResources() {
            var resources = await HttpUtil.RetryAsync<Resources>(() => _uri.AppendPathSegment("resources")
                                                                                  .SetQueryParam($"lang={GameService.Overlay.UserLocale.Value.TwoLetterISOLanguageName()}")
                                                                                  .GetAsync());
            return resources ?? Resources.Empty;
        }
    }
}
