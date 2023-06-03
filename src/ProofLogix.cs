using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Nekres.ProofLogix.Core.Services;
using Nekres.ProofLogix.Core.UI.Table;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Nekres.ProofLogix.Core.UI.LookingForOpener;

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

        internal ResourceService  Resources;
        internal KpWebApiService  KpWebApi;
        internal PartySyncService PartySync;

        private TableConfig    _tableConfig;
        private LfoConfig      _lfoConfig;

        private TabbedWindow2  _window;
        private CornerIcon     _cornerIcon;
        private AsyncTexture2D _icon;

        protected override void DefineSettings(SettingCollection settings) {
        }

        protected override void Initialize() {
            Resources = new ResourceService();
            KpWebApi  = new KpWebApiService();
            PartySync = new PartySyncService();
        }

        protected override async Task LoadAsync() {
            await Resources.LoadAsync();
            await PartySync.InitSquad();
        }

        protected override void OnModuleLoaded(EventArgs e) {
            GameService.ArcDps.Common.Activate();

            _icon = ContentsManager.GetTexture("killproof_icon.png");
            _cornerIcon = new CornerIcon(_icon, "Kill Proof") {
                Priority = 236278055 // Arbitrary value that should be unique to this module.
            };

            _window = new TabbedWindow2(GameService.Content.DatAssetCache.GetTextureFromAssetId(155985), 
                                         new Rectangle(40, 26, 913, 691), 
                                         new Rectangle(100, 36, 839, 605)) {
                Parent        = GameService.Graphics.SpriteScreen,
                Title         = this.Name,
                Emblem        = _icon,
                Subtitle      = "Kill Proof",
                SavesPosition = true,
                Id            = $"{nameof(ProofLogix)}_KillProof_91702dd39f0340b5bd7883cc566e4f63",
                CanResize = true,
                Width = 700,
                Height = 600
            };

            _tableConfig = new TableConfig();
            _window.Tabs.Add(new Tab(GameService.Content.DatAssetCache.GetTextureFromAssetId(156407), () => new TableView(_tableConfig), "Squad Tracker"));

            _lfoConfig = new LfoConfig();
            _window.Tabs.Add(new Tab(GameService.Content.DatAssetCache.GetTextureFromAssetId(156680), () => new LfoView(_lfoConfig), "Looking for Opener"));

            _window.Show();

            _cornerIcon.Click += OnCornerIconClick;
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private void OnCornerIconClick(object sender, MouseEventArgs e) {
            _window.ToggleWindow();
        }

        /// <inheritdoc />
        protected override void Unload() {
            _cornerIcon.Click -= OnCornerIconClick;
            _cornerIcon?.Dispose();
            _window?.Dispose();
            _icon?.Dispose();

            PartySync.Dispose();
            Resources.Dispose();

            // All static members must be manually unset
            Instance = null;
        }
    }
}
