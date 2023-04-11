using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Nekres.ProofLogix.Core.Utils;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1 {
    internal class KpV1Client {

        private string _uri = "https://killproof.me/api/{0}/{1}";

        public async Task<Profile> GetAccount(string id) {

            var request = string.Format(_uri, "kp", id);

            var account = await HttpUtil.RetryAsync<Profile>(request);

            if (account == null) {
                return null;
            }

            account.Clears = await GetClears(id);

            return account;
        }

        public async Task<List<Raid>> GetClears(string id) {

            var request  = string.Format(_uri, "clear", id);

            var response = await HttpUtil.RetryAsync<JObject>(request);

            var clears = response.Properties()
                                 .Select(property => new JObject { [property.Name] = property.Value }.ToObject<Raid>())
                                 .ToList();

            return clears;
        }
    }
}
