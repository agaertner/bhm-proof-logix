using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.UI.Configs {
    public class SmartPingConfig : ConfigBase {

        public static SmartPingConfig Default => new() {
            _selectedToken = 77302
        };

        private int _selectedToken;
        [JsonProperty("selected_token")]
        public int SelectedToken {
            get => _selectedToken;
            set {
                _selectedToken = value;
                SaveConfig(ProofLogix.Instance.SmartPingConfig);
            }
        }
    }
}
