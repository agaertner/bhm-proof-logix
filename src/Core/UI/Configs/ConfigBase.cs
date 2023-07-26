using Blish_HUD.Settings;

namespace Nekres.ProofLogix.Core.UI.Configs {
    public abstract class ConfigBase {
        protected void SaveConfig<T>(SettingEntry<T> setting) where T : ConfigBase {
            if (setting?.IsNull ?? true) {
                return;
            }
            setting.Value = null;
            setting.Value = this as T;
        }
    }
}
