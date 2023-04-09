using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Nekres.ProofLogix.Core.Utils;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.Services {
    internal class KpWebApiService {

        private string _uri = "https://killproof.me/api/";

        public async Task<Account> GetAccount(string id) {
            var account = await TaskUtil.RetryAsync<Account>($"{_uri}kp/{id}");
            return account;
        }
    }
}
