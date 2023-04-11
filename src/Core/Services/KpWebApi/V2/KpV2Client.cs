using Blish_HUD;
using Blish_HUD.Extended;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using Nekres.ProofLogix.Core.Utils;
using System.Threading.Tasks;
namespace Nekres.ProofLogix.Core.Services.KpWebApi.V2 {
    internal class KpV2Client {

        private readonly string _uri = "https://killproof.me/api/{0}/{1}?lang={2}";

        public async Task<Profile> GetProfile(string id) {

            var request = string.Format(_uri, "kp", id, GameService.Overlay.UserLocale.Value.Code());
            
            return await HttpUtil.RetryAsync<Profile>(request);
        }
    }
}
