using Blish_HUD.ArcDps.Common;
using Blish_HUD.Content;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using System;
using System.Threading.Tasks;
using Blish_HUD;

namespace Nekres.ProofLogix.Core.Services.PartySync.Models {
    public sealed class Player {

        public Profile KpProfile { get; private set; }
        public bool    IsLoading { get; private set; }

        public string         Class         => GetClass();
        public AsyncTexture2D Icon          => GetIcon();
        public string         AccountName   => _arcDpsPlayer.AccountName;
        public string         CharacterName => _arcDpsPlayer.CharacterName;
        public bool           Self          => _arcDpsPlayer.Self;

        private readonly CommonFields.Player _arcDpsPlayer;
        private readonly Func<Task<Profile>> _requestProfile;

        public Player(CommonFields.Player arcDpsPlayer, Func<Task<Profile>> requestProfile) {
            _arcDpsPlayer   = arcDpsPlayer;
            _requestProfile = requestProfile;
        }

        public async Task LoadAsync() {
            this.IsLoading = true;
            this.KpProfile = await _requestProfile().ConfigureAwait(false);
            this.IsLoading = false;
        }

        private string GetClass() {
            return PartySyncService.EliteNames.TryGetValue((int)_arcDpsPlayer.Elite, out var name) ? name :
                   PartySyncService.ProfNames.TryGetValue((int)_arcDpsPlayer.Profession, out name) ? name : string.Empty;
        }

        private AsyncTexture2D GetIcon() {
            return PartySyncService.EliteIcons.TryGetValue((int)_arcDpsPlayer.Elite, out var icon) ? icon :
                   PartySyncService.ProfIcons.TryGetValue((int)_arcDpsPlayer.Profession, out icon) ? icon : ContentService.Textures.TransparentPixel;
        }
    }
}
