using System.Collections.Generic;
using System.Linq;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Nekres.ProofLogix.Core.Utils;
using System.Threading.Tasks;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models.Converter;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.Services {
    internal class KpWebApiService {

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
