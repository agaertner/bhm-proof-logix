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
    public class KpV1Client {

        private readonly string _uri = "https://killproof.me/api/";

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
            return response?.Status.Equals("ok") ?? false;
        }

        public async Task<bool> CheckProofBusy(string id) {
            var busy = await HttpUtil.RetryAsync<ProofBusy>(() => $"https://killproof.me/proofbusy/{id}".GetAsync());
            return busy.Busy != 2;
        }

        public async Task<Opener> GetOpener(string encounter, Opener.ServerRegion region) {
            var response = await HttpUtil.RetryAsync<Opener>(() => _uri.AppendPathSegment("opener")
                                                                       .SetQueryParams($"encounter={encounter}", $"region={region}").GetAsync());

            if (response == null) {
                return Opener.Empty;
            }

            return response.Volunteers?.Any() ?? false ? response : Opener.Empty;
        }

        public async Task<AddKey> AddKey(string apiKey, bool opener) {

            var response = await HttpUtil.RetryAsync<AddKey>(() => _uri.AppendPathSegment("addkey")
                                                                       .PostJsonAsync(new JObject {
                                                                            ["key"]    = apiKey,
                                                                            ["opener"] = Convert.ToInt32(opener)
                                                                        }));
            return response ?? new AddKey {
                Error = "No response."
            };
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
