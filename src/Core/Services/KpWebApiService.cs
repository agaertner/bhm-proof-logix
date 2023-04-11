using Nekres.ProofLogix.Core.Services.KpWebApi.V2;
using System.Threading.Tasks;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;

namespace Nekres.ProofLogix.Core.Services {
    internal class KpWebApiService {

        private KpV2Client _client;

        public KpWebApiService() {
             _client = new KpV2Client();
        }

        public async Task<Account> GetAccount(string id) {
            return await _client.GetAccount(id);
        }
    }
}
