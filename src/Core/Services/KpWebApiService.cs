using Nekres.ProofLogix.Core.Services.KpWebApi.V1;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using System.Threading.Tasks;
using Profile = Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models.Profile;

namespace Nekres.ProofLogix.Core.Services {
    internal class KpWebApiService {

        private readonly KpV1Client _v1Client;
        private readonly KpV2Client _v2Client;
       
        public KpWebApiService() {
            _v1Client = new KpV1Client();
            _v2Client = new KpV2Client();
        }

        public async Task<Profile> GetProfile(string id) {
            if (string.IsNullOrEmpty(id)) {
                return Profile.Empty;
            }

            var profile = await _v2Client.GetProfile(id);

            return await ExpandProfile(profile);
        }

        public async Task<Profile> GetProfileByCharacterName(string characterName) {
            if (string.IsNullOrEmpty(characterName)) {
                return Profile.Empty;
            }

            var profile = await _v2Client.GetProfileByCharacterName(characterName);

            return await ExpandProfile(profile);
        }

        private async Task<Profile> ExpandProfile(Profile profile) {
            if (profile.IsEmpty) {
                return profile;
            }

            profile.Clears = await _v1Client.GetClears(profile.Id);

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
