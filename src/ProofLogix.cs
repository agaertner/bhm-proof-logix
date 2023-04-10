using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Nekres.ProofLogix.Core.Services;

namespace Nekres.ProofLogix {
    [Export(typeof(Module))]
    public class ProofLogix : Module {
        internal static readonly Logger Logger = Logger.GetLogger<ProofLogix>();

        internal static ProofLogix Instance { get; private set; }

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        [ImportingConstructor]
        public ProofLogix([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) => Instance = this;

        internal KpWebApiService KpWebApi;

        protected override void DefineSettings(SettingCollection settings) {
        }

        protected override void Initialize() {
            KpWebApi = new KpWebApiService();
        }

        protected override async Task LoadAsync() {
            var account = await KpWebApi.GetAccount("Nekres.1943");
            
            System.Console.WriteLine("cool");
        }

        protected override void OnModuleLoaded(EventArgs e) {
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        /// <inheritdoc />
        protected override void Unload() {
            // All static members must be manually unset
            Instance = null;
        }
    }
}
