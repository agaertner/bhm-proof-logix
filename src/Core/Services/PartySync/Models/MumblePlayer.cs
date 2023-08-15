using Blish_HUD;

namespace Nekres.ProofLogix.Core.Services.PartySync.Models {
    /// <summary>
    /// Represents the local player driven by <see cref="Gw2MumbleService"/> contextual data.
    /// </summary>
    public sealed class MumblePlayer : Player {
        public override string CharacterName => GameService.Gw2Mumble.PlayerCharacter.Name;

        public override OnlineStatus Status => GetStatus();

        protected override int GetSpecialization() {
            return GameService.Gw2Mumble.PlayerCharacter.Specialization;
        }

        protected override int GetProfession() {
            return (int)GameService.Gw2Mumble.PlayerCharacter.Profession;
        }

        public OnlineStatus GetStatus() {
            // Mumble is active.
            if (GameService.GameIntegration.Gw2Instance.IsInGame) {
                return OnlineStatus.Online;
            }

            // Never went past character select.
            if (string.IsNullOrWhiteSpace(GameService.Gw2Mumble.PlayerCharacter.Name)) {
                return OnlineStatus.Unknown;
            }

            // Mumble was active before.
            return OnlineStatus.Away;
        }

        public MumblePlayer() : base() {
            /* NOOP */
        }
    }
}
