using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Nekres.ProofLogix.Core.Utils;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1 {
    internal class KpV1Client {

        private string _uri = "https://killproof.me/api/";

        public async Task<Account> GetAccount(string id) {
            var account = await TaskUtil.RetryAsync<Account>($"{_uri}kp/{id}");

            if (account == null) {
                return null;
            }

            var response = await TaskUtil.RetryAsync<JObject>($"{_uri}clear/{id}");

            var clears = response.Properties()
                                 .Select(property => new JObject { [property.Name] = property.Value }.ToObject<Raid>())
                                 .ToList();

            account.Clears = clears;

            return account;
        }
    }
}
