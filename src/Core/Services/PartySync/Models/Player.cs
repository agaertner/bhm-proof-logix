using Blish_HUD.ArcDps.Common;
using Blish_HUD.Content;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using System;
using System.Threading.Tasks;
using Blish_HUD;

namespace Nekres.ProofLogix.Core.Services.PartySync.Models {
    public sealed class Player {

        public Profile KpProfile   { get; private set; }
        public bool    IsLoading   { get; private set; }
        public string  AccountName { get; private set; }

        public string         Class         => GetClass();
        public AsyncTexture2D Icon          => GetIcon();
        public string         CharacterName => _arcDpsPlayer.CharacterName;
        public bool           Self          => _arcDpsPlayer.Self;
        public bool           HasAgent      => string.IsNullOrEmpty(_arcDpsPlayer.AccountName);

        private CommonFields.Player _arcDpsPlayer;

        private Player() {
            this.IsLoading = true;
        }

        public Player(string accountName) : this() {
            this.AccountName = accountName;
        }

        public static Player FromArcDps(CommonFields.Player arcDpsPlayer) {
            return new Player(arcDpsPlayer.AccountName) {
                _arcDpsPlayer = arcDpsPlayer
            };
        }

        public bool AttachAgent(CommonFields.Player arcDpsPlayer) {
            if (string.IsNullOrEmpty(this.AccountName)) {
                return false;
            }

            if (!this.AccountName.Equals(arcDpsPlayer.AccountName, StringComparison.InvariantCultureIgnoreCase)) {
                return false;
            }

            _arcDpsPlayer    = arcDpsPlayer;
            this.AccountName = arcDpsPlayer.AccountName;
            
            return true;
        }

        public async Task LoadAsync() {
            this.IsLoading = true;
            this.KpProfile = await ProofLogix.Instance.KpWebApi.GetProfile(this.AccountName).ConfigureAwait(false);
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
