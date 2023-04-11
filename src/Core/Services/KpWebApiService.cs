using Nekres.ProofLogix.Core.Services.KpWebApi.V1;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.Services {
    internal class KpWebApiService {

        private readonly KpV1Client _v1Client;
        private readonly KpV2Client _v2Client;
       
        public KpWebApiService() {
            _v1Client = new KpV1Client();
            _v2Client = new KpV2Client();
        }

        public async Task<Profile> GetProfile(string id) {
            var profile = await _v2Client.GetProfile(id);

            if (profile == null) {
                return Profile.Empty;
            }

            profile.Clears = await _v1Client.GetClears(id);

            if (profile.Linked == null) {
                return profile;
            }

            foreach (var link in profile.Linked) {
                link.Clears = await _v1Client.GetClears(link.Id);
            }

            return profile;
        }
    }
}
