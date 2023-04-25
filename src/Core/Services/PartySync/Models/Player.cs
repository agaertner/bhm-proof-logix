using Blish_HUD;
using Blish_HUD.ArcDps.Common;
using Blish_HUD.Content;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using System;

namespace Nekres.ProofLogix.Core.Services.PartySync.Models {
    public sealed class Player {

        public Profile KpProfile     { get; private set; }
        public string  AccountName   { get; private set; }
        public bool    IsLocalPlayer { get; private set; }
        public string  CharacterName { get; private set; }

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

        public static Player FromKpProfile(Profile profile, bool isLocalPlayer = false) {
            return new Player(profile.Name) {
                KpProfile = profile,
                IsLocalPlayer = isLocalPlayer,
                CharacterName = isLocalPlayer ? GameService.Gw2Mumble.PlayerCharacter.Name : string.Empty
            };
        }

        public bool AttachAgent(CommonFields.Player arcDpsPlayer) {
            if (!this.AccountName.Equals(arcDpsPlayer.AccountName, StringComparison.InvariantCultureIgnoreCase)) {
                return false;
            }

            _arcDpsPlayer      = arcDpsPlayer;
            this.AccountName   = arcDpsPlayer.AccountName;
            this.CharacterName = arcDpsPlayer.CharacterName;
            this.IsLocalPlayer = this.IsLocalPlayer || arcDpsPlayer.Self;

            return true;
        }

        public bool AttachProfile(Profile kpProfile, bool isLocalPlayer = false) {
            if (!this.AccountName.Equals(kpProfile.Name, StringComparison.InvariantCultureIgnoreCase)) {
                return false;
            }

            this.KpProfile     = kpProfile;
            this.AccountName   = kpProfile.Name;
            this.IsLocalPlayer = this.IsLocalPlayer || isLocalPlayer;
            this.CharacterName = this.IsLocalPlayer ? GameService.Gw2Mumble.PlayerCharacter.Name : this.CharacterName;

            return true;
        }

        private string GetClass() {
            var elite      = this.IsLocalPlayer ? GameService.Gw2Mumble.PlayerCharacter.Specialization : (int)_arcDpsPlayer.Elite;
            var profession = this.IsLocalPlayer ? (int)GameService.Gw2Mumble.PlayerCharacter.Profession : (int)_arcDpsPlayer.Profession;
            return ResourceService.GetClassName(profession, elite);
        }

        private AsyncTexture2D GetIcon() {
            var elite      = this.IsLocalPlayer ? GameService.Gw2Mumble.PlayerCharacter.Specialization : (int)_arcDpsPlayer.Elite;
            var profession = this.IsLocalPlayer ? (int)GameService.Gw2Mumble.PlayerCharacter.Profession : (int)_arcDpsPlayer.Profession;
            return ResourceService.GetClassIcon(profession, elite);
        }
    }
}
