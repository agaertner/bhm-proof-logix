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

        private TableConfig    _config;
        private StandardWindow _window;
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
                Priority = 296658677 // Arbitrary value that should be unique to this module.
            };

            _window = new StandardWindow(GameService.Content.DatAssetCache.GetTextureFromAssetId(155985), 
                                         new Rectangle(40, 26, 913, 691), 
                                         new Rectangle(70, 71, 839, 605)) {
                Parent        = GameService.Graphics.SpriteScreen,
                Title         = this.Name,
                Emblem        = _icon,
                Subtitle      = "Kill Proof",
                SavesPosition = true,
                Id            = $"{nameof(ProofLogix)}_8af48717-08ee-43e8-9ed8-aab24f53ab9c",
                CanResize = true,
                Width = 1000,
                Height = 400
            };

            _config = new TableConfig();
            _window.Show(new TableView(_config));

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
