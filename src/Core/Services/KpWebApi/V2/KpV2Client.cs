using Blish_HUD;
using Blish_HUD.Extended;
using Flurl;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using Nekres.ProofLogix.Core.Utils;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V2 {
    internal class KpV2Client {

        private readonly string _uri = "https://killproof.me/api/";

        public async Task<Profile> GetProfile(string id) {

            var request = _uri.AppendPathSegments("kp", id)
                                  .SetQueryParam($"lang={GameService.Overlay.UserLocale.Value.Code()}");
            
            return await HttpUtil.RetryAsync<Profile>(request);
        }
    }
}
