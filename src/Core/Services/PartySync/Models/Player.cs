using Blish_HUD.ArcDps.Common;
using Blish_HUD.Content;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using System;

namespace Nekres.ProofLogix.Core.Services.PartySync.Models {
    /// <summary>
    /// Represents a player driven by <see href="www.killproof.me"/> profile<br/>
    /// and <see cref="CommonFields.Player"/> agent data if such data sources are attached.
    /// </summary>
    public class Player {

        public Profile KpProfile     { get; private set; }

        public DateTime Created { get; private set; }

        public string  AccountName => HasAgent ? _arcDpsPlayer.AccountName : 
                                      HasKpProfile ? KpProfile.Name : string.Empty;

        public virtual string CharacterName => _arcDpsPlayer.CharacterName ?? string.Empty;

        public bool HasAgent     => !string.IsNullOrEmpty(_arcDpsPlayer.AccountName);
        public bool HasKpProfile => this.KpProfile is {NotFound: false};

        public string         Class => GetClass();
        public AsyncTexture2D Icon  => GetIcon();


        private CommonFields.Player _arcDpsPlayer;

        public Player() {
            this.Created   = DateTime.UtcNow;
            this.KpProfile = Profile.Empty;
        }

        public Player(CommonFields.Player agent) : this() {
            _arcDpsPlayer = agent;
        }

        public Player(Profile profile) : this() {
            this.KpProfile = profile;
        }

        public bool Equals(Player other) {
            return this.AccountName.Equals(other.AccountName, StringComparison.InvariantCultureIgnoreCase)
                || this.HasKpProfile && this.KpProfile.BelongsTo(other.AccountName, out _);
        }

        public void AttachAgent(CommonFields.Player arcDpsPlayer) {
            _arcDpsPlayer = arcDpsPlayer;
        }

        public void AttachProfile(Profile kpProfile) {
            this.KpProfile = kpProfile;
        }

        protected virtual int GetSpecialization() {
            return (int)_arcDpsPlayer.Elite;
        }

        protected virtual int GetProfession() {
            return (int)_arcDpsPlayer.Profession;
        }

        private string GetClass() {
            return ProofLogix.Instance.Resources.GetClassName(GetProfession(), GetSpecialization());
        }

        private AsyncTexture2D GetIcon() {
            return ProofLogix.Instance.Resources.GetClassIcon(GetProfession(), GetSpecialization());
        }
    }
}
