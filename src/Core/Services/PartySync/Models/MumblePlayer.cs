using Blish_HUD;

namespace Nekres.ProofLogix.Core.Services.PartySync.Models {
    /// <summary>
    /// Represents the local player driven by <see cref="Gw2MumbleService"/> contextual data.
    /// </summary>
    public sealed class MumblePlayer : Player {
        public override string CharacterName => GameService.Gw2Mumble.PlayerCharacter.Name;

        protected override int GetSpecialization() {
            return GameService.Gw2Mumble.PlayerCharacter.Specialization;
        }

        protected override int GetProfession() {
            return (int)GameService.Gw2Mumble.PlayerCharacter.Profession;
        }
    }
}
