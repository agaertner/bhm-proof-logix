using Flurl;
using Flurl.Http;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Nekres.ProofLogix.Core.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V1 {
    internal class KpV1Client {

        private readonly string _uri = "https://killproof.me/api/";

        private readonly IReadOnlyList<IReadOnlyList<string>> _wings = new List<IReadOnlyList<string>> {
            new List<string> { "vale_guardian", "spirit_woods", "gorseval", "sabetha" },
            new List<string> { "slothasor", "bandit_trio", "matthias" },
            new List<string> { "escort", "keep_construct", "twisted_castle", "xera" },
            new List<string> { "cairn", "mursaat_overseer", "samarog", "deimos" },
            new List<string> { "soulless_horror", "river_of_souls", "statues_of_grenth", "voice_in_the_void" },
            new List<string> { "conjured_amalgamate", "twin_largos", "qadim" },
            new List<string> { "gate", "adina", "sabir", "qadim_the_peerless" },
        };

        public async Task<Profile> GetProfile(string id) {
            var profile = await HttpUtil.RetryAsync<Profile>(() => _uri.AppendPathSegments("kp", id).GetAsync());
            return profile ?? Profile.Empty;
        }

        public async Task<Profile> GetProfileByCharacter(string name) {
            var profile = await HttpUtil.RetryAsync<Profile>(() => _uri.AppendPathSegments("character", name, "kp").GetAsync());
            return profile ?? Profile.Empty;
        }

        public async Task<List<Clear>> GetClears(string id) {
            var response = await HttpUtil.RetryAsync<JObject>(() => _uri.AppendPathSegments("clear", id).GetAsync());
            return FormatClears(response);
        }

        public async Task<List<Clear>> GetClearsByCharacter(string name) {
            var response = await HttpUtil.RetryAsync<JObject>(() => _uri.AppendPathSegments("character", name, "clear").GetAsync());
            return FormatClears(response);
        }

        public async Task<bool> Refresh(string id) {
            var response = await HttpUtil.RetryAsync<Refresh>(() => $"https://killproof.me/proof/{id}/refresh".GetAsync());

            return response is {Status: HttpStatusCode.OK};
        }

        public async Task<Opener> GetOpener(string encounter, Opener.ServerRegion region) {

            var encounters = _wings.SelectMany(x => x);

            if (!encounters.Any(x => x.Equals(encounter))) {
                return Opener.Empty;
            }

            var response = await HttpUtil.RetryAsync<Opener>(() => _uri.AppendPathSegment("opener")
                                                                       .SetQueryParams($"encounter={encounter}", $"region={region}").GetAsync());

            if (response == null) {
                return Opener.Empty;
            }

            return response.Volunteers?.Any() ?? false ? response : Opener.Empty;

        }

        public async Task<string> AddKey(string apiKey, bool opener) {

            var response = await HttpUtil.RetryAsync<AddKey>(() => _uri.AppendPathSegment("addkey")
                                                                       .PostJsonAsync(new JObject {
                                                                            ["key"]    = apiKey,
                                                                            ["opener"] = Convert.ToInt32(opener)
                                                                        }));

            if (response == null) {
                return string.Empty;
            }

            if (response.IsError) {
                ProofLogix.Logger.Trace(response.Error);
                return string.Empty;
            }

            return response.KpId;
        }

        private static List<Clear> FormatClears(JObject response) {
            if (response == null) {
                return Enumerable.Empty<Clear>().ToList();
            }

            return response.Properties()
                           .Select(property => new JObject {
                                [property.Name] = property.Value
                            }.ToObject<Clear>())
                           .ToList();
        } 
    }
}
