using Nekres.ProofLogix.Core.Utils;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using Raid = Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models.Raid;
namespace Nekres.ProofLogix.Core.Services.KpWebApi.V2 {
    internal class KpV2Client {

        private string _uri = "https://killproof.me/api/{0}/{1}?lang={2}";

        public async Task<Account> GetAccount(string id) {
            var account = await TaskUtil.RetryAsync<Account>(string.Format(_uri, "kp", id, GameService.Overlay.UserLocale.Value.Code()));

            if (account == null) {
                return null;
            }

            var response = await TaskUtil.RetryAsync<JObject>(string.Format(_uri, "clear", id, GameService.Overlay.UserLocale.Value.Code()));

            var clears = response.Properties()
                                 .Select(property => new JObject { [property.Name] = property.Value }.ToObject<Raid>())
                                 .ToList();

            account.Clears = clears;

            return account;
        }
    }
}
