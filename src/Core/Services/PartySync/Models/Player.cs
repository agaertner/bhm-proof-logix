using Blish_HUD;
using Blish_HUD.ArcDps.Common;
using Blish_HUD.Content;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using System;

namespace Nekres.ProofLogix.Core.Services.PartySync.Models {
    public sealed class Player {

        public Profile KpProfile     { get; private set; }
        public string  AccountName   { get; private set; }
        public bool    IsLocalPlayer { get; set; }
        public string  CharacterName { get; set; }

        public bool           HasAgent     => !string.IsNullOrEmpty(_arcDpsPlayer.AccountName);
        public bool           HasKpProfile => this.KpProfile != null;
        public string         Class        => GetClass();
        public AsyncTexture2D Icon         => GetIcon();
        
        private CommonFields.Player _arcDpsPlayer;

        private Player() {
            /* NOOP */
        }

        public Player(string accountName) : this() {
            this.AccountName = accountName ?? string.Empty;
        }

        public static Player FromArcDps(CommonFields.Player arcDpsPlayer) {
            return new Player(arcDpsPlayer.AccountName) {
                _arcDpsPlayer = arcDpsPlayer,
                CharacterName = arcDpsPlayer.CharacterName,
                IsLocalPlayer = arcDpsPlayer.Self
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

            _arcDpsPlayer      = arcDpsPlayer;
            this.AccountName   = arcDpsPlayer.AccountName;
            this.CharacterName = arcDpsPlayer.CharacterName;
            this.IsLocalPlayer = arcDpsPlayer.Self;

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
            return ResourceService.EliteNames.TryGetValue((int)_arcDpsPlayer.Elite, out var name) ? name :
                   ResourceService.ProfNames.TryGetValue((int)_arcDpsPlayer.Profession, out name) ? name : string.Empty;
        }

        private AsyncTexture2D GetIcon() {
            return ResourceService.EliteIcons.TryGetValue((int)_arcDpsPlayer.Elite, out var icon) ? icon :
                   ResourceService.ProfIcons.TryGetValue((int)_arcDpsPlayer.Profession, out icon) ? icon : ContentService.Textures.TransparentPixel;
        }
    }
}
