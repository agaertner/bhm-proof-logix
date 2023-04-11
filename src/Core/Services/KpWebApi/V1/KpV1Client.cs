using System;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Nekres.ProofLogix.Core.Utils;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1 {
    internal class KpV1Client {

        private readonly string _uri = "https://killproof.me/api/";

        private IReadOnlyDictionary<int, IReadOnlyList<string>> _wings = new Dictionary<int, IReadOnlyList<string>> {
            {1, new List<string> { "vale_guardian", "spirit_woods", "gorseval", "sabetha" }},
            {2, new List<string> { "slothasor", "bandit_trio", "matthias" }},
            {3, new List<string> { "escort", "keep_construct", "twisted_castle", "xera" }},
            {4, new List<string> { "cairn", "mursaat_overseer", "samarog", "deimos" }},
            {5, new List<string> { "soulless_horror", "river_of_souls", "statues_of_grenth", "voice_in_the_void" }},
            {6, new List<string> { "conjured_amalgamate", "twin_largos", "qadim" }},
            {7, new List<string> { "gate", "adina", "sabir", "qadim_the_peerless" }},
        };

        public async Task<Profile> GetAccount(string id) {
            var profile = await HttpUtil.RetryAsync<Profile>(() => _uri.AppendPathSegments("kp", id).GetAsync());

            if (profile == null) {
                return Profile.Empty;
            }

            profile.Clears = await GetClears(id);

            return profile;
        }

        public async Task<List<Raid>> GetClears(string id) {
            var response = await HttpUtil.RetryAsync<JObject>(() => _uri.AppendPathSegments("clear", id).GetAsync());

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

        public async Task<bool> Refresh(string id) {
            var response = await HttpUtil.RetryAsync<Refresh>(() => $"https://killproof.me/proof/{id}/refresh".GetAsync());

            return response is {Status: HttpStatusCode.OK};
        }

        public async Task<Opener> GetOpener(string encounter, string serverRegion) {
            if (!Enum.TryParse<Opener.ServerRegion>(serverRegion, true, out var region)) {
                return null;
            }

            var encounters = _wings.Values.SelectMany(x => x);

            if (!encounters.Any(x => x.Equals(encounter))) {
                return null;
            }

            var response = await HttpUtil.RetryAsync<Opener>(() => _uri.AppendPathSegment("opener")
                                                                       .SetQueryParams($"encounter={encounter}", $"region={region}").GetAsync());

            return response ?? Opener.Empty;

        }

        public async Task<bool> AddKey(string apiKey, bool opener) {

            var response = await HttpUtil.RetryAsync<AddKey>(() => _uri.AppendPathSegment("addkey")
                                                                       .PostJsonAsync(new JObject {
                                                                            ["key"]    = apiKey,
                                                                            ["opener"] = Convert.ToInt32(opener)
                                                                        }));

            if (response == null) {
                return false;
            }

            if (string.IsNullOrEmpty(response.Error)) {
                return true;
            }

            ProofLogix.Logger.Trace(response.Error);
            return false;
        }
    }
}
