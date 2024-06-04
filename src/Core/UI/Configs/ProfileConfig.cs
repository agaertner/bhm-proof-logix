using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.UI.Configs {
    internal class ProfileConfig : ConfigBase {
        public static ProfileConfig Default => new() {
            _selectedTab = 1
        };

        private int _selectedTab;
        [JsonProperty("selected_tab")]
        public int SelectedTab {
            get => _selectedTab;
            set {
                _selectedTab = value;
                this.SaveConfig(ProofLogix.Instance.ProfileConfig);
            }
        }
    }
}
