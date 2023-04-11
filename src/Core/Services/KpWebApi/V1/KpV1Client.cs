using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Nekres.ProofLogix.Core.Utils;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1 {
    internal class KpV1Client {

        private readonly string _uri = "https://killproof.me/api/{0}/{1}";

        public async Task<Profile> GetAccount(string id) {

            var request = string.Format(_uri, "kp", id);

            var profile = await HttpUtil.RetryAsync<Profile>(request);

            if (profile == null) {
                return Profile.Empty;
            }

            profile.Clears = await GetClears(id);

            return profile;
        }

        public async Task<List<Raid>> GetClears(string id) {

            var request  = string.Format(_uri, "clear", id);

            var response = await HttpUtil.RetryAsync<JObject>(request);

            if (response == null) {
                return Enumerable.Empty<Raid>().ToList();
            }

            var clears = response.Properties()
                                 .Select(property => new JObject {
                                      [property.Name] = property.Value
                                  }.ToObject<Raid>())
                                 .ToList();

            return clears;
        }
    }
}
