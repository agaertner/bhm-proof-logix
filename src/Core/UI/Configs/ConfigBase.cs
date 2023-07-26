using Blish_HUD.Settings;

namespace Nekres.ProofLogix.Core.UI.Configs {
    public abstract class ConfigBase {
        protected void SaveConfig<T>(SettingEntry<T> setting) where T : ConfigBase {
            if (setting?.IsNull ?? true) {
                return;
            }
            // unset value first otherwise the references (old vs. new) would
            // be the same and thus not pass as a property change required to invoke a save.
            setting.Value = null; 
            setting.Value = this as T; 
        }
    }
}
