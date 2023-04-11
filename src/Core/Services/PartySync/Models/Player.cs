using Blish_HUD;
using Blish_HUD.ArcDps.Common;
using Blish_HUD.Content;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using System;

namespace Nekres.ProofLogix.Core.Services.PartySync.Models {
    public sealed class Player {

        public Profile KpProfile   { get; private set; }
        public string  AccountName { get; private set; }

        public bool           HasAgent      => !string.IsNullOrEmpty(_arcDpsPlayer.AccountName);
        public bool           HasKpProfile  => !KpProfile?.IsEmpty ?? false;
        public string         Class         => GetClass();
        public AsyncTexture2D Icon          => GetIcon();
        public string         CharacterName => _arcDpsPlayer.CharacterName;
        public bool           Self          => _arcDpsPlayer.Self;

        private CommonFields.Player _arcDpsPlayer;

        private Player() {
            /* NOOP */
        }

        public Player(string accountName) : this() {
            this.AccountName = accountName ?? string.Empty;
        }

        public static Player FromArcDps(CommonFields.Player arcDpsPlayer) {
            return new Player(arcDpsPlayer.AccountName) {
                _arcDpsPlayer = arcDpsPlayer
            };
        }

        public static Player FromKpProfile(Profile profile) {
            return new Player(profile.Name) {
                KpProfile = profile
            };
        }

        public bool AttachAgent(CommonFields.Player arcDpsPlayer) {
            if (!this.AccountName.Equals(arcDpsPlayer.AccountName, StringComparison.InvariantCultureIgnoreCase)) {
                return false;
            }

            _arcDpsPlayer    = arcDpsPlayer;
            this.AccountName = arcDpsPlayer.AccountName;
            
            return true;
        }

        public bool AttachProfile(Profile kpProfile) {
            if (!this.AccountName.Equals(kpProfile.Name, StringComparison.InvariantCultureIgnoreCase)) {
                return false;
            }

            this.KpProfile   = kpProfile;
            this.AccountName = kpProfile.Name;
            return true;
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
