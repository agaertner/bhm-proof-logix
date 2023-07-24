using System;
using System.Collections.Generic;
using System.Linq;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using System.Threading.Tasks;
using Blish_HUD;
using Gw2Sharp.WebApi.V2.Models;
using Nekres.ProofLogix.Core.Services.KpWebApi.V1.Models;
using Profile = Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models.Profile;

namespace Nekres.ProofLogix.Core.Services {
    internal class KpWebApiService : IDisposable {

        public static event EventHandler<ValueEventArgs<bool>> SubtokenUpdated;

        private readonly KpV1Client _v1Client;
        private readonly KpV2Client _v2Client;

        public readonly IReadOnlyList<TokenPermission> RequiredPermissions = new List<TokenPermission> {
            TokenPermission.Account,
            TokenPermission.Inventories,
            TokenPermission.Characters,
            TokenPermission.Wallet,
            TokenPermission.Unlocks,
            TokenPermission.Progression
        };

        public KpWebApiService() {
            _v1Client = new KpV1Client();
            _v2Client = new KpV2Client();

            ProofLogix.Instance.Gw2ApiManager.SubtokenUpdated += OnSubtokenUpdated;
        }

        public void Dispose() {
            ProofLogix.Instance.Gw2ApiManager.SubtokenUpdated -= OnSubtokenUpdated;
        }

        private void OnSubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e) {
            // Checks token for insufficient permissions.
            var valid = e.Value.Intersect(RequiredPermissions).Count() == RequiredPermissions.Count;
            SubtokenUpdated?.Invoke(this, new ValueEventArgs<bool>(valid));
        }

        public async Task<AddKey> AddKey(string key, bool opener) {
            return await _v1Client.AddKey(key, opener);
        }

        public async Task<Opener> GetOpener(string encounterId, Opener.ServerRegion region) {
            return await _v1Client.GetOpener(encounterId, region);
        }

        public async Task<Resources> GetResources() {
            return await _v2Client.GetResources();
        }

        public async Task<bool> Refresh(string id) {
            return await _v1Client.Refresh(id);
        }

        public async Task<bool> IsProofBusy(string id) {
            return await _v1Client.CheckProofBusy(id);
        }

        public async Task<Profile> GetProfile(string id) {
            if (string.IsNullOrEmpty(id)) {
                return Profile.Empty;
            }

            var profile = await _v2Client.GetProfile(id);

            return await ExpandProfile(profile);
        }

        public async Task<Profile> GetProfileByCharacter(string characterName) {
            if (string.IsNullOrEmpty(characterName)) {
                return Profile.Empty;
            }

            var profile = await _v2Client.GetProfileByCharacter(characterName);

            return await ExpandProfile(profile);
        }

        private async Task<Profile> ExpandProfile(Profile profile) {
            if (profile.NotFound) {
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
