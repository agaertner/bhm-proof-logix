using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Nekres.ProofLogix.Core.Services;
using Nekres.ProofLogix.Core.UI;
using Nekres.ProofLogix.Core.UI.Configs;
using Nekres.ProofLogix.Core.UI.Home;
using Nekres.ProofLogix.Core.UI.LookingForOpener;
using Nekres.ProofLogix.Core.UI.Table;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Gw2WebApiService = Nekres.ProofLogix.Core.Services.Gw2WebApiService;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
namespace Nekres.ProofLogix {
    [Export(typeof(Module))]
    public class ProofLogix : Module {
        internal static readonly Logger Logger = Logger.GetLogger<ProofLogix>();

        internal static ProofLogix Instance { get; private set; }

        #region Service Managers
        internal SettingsManager    SettingsManager     => this.ModuleParameters.SettingsManager;
        internal ContentsManager    ContentsManager     => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager  => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager      Gw2ApiManager       => this.ModuleParameters.Gw2ApiManager;
        #endregion

        [ImportingConstructor]
        public ProofLogix([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) => Instance = this;

        internal ResourceService  Resources;
        internal KpWebApiService  KpWebApi;
        internal PartySyncService PartySync;
        internal Gw2WebApiService Gw2WebApi;

        private TabbedWindow2     _window;
        private OversizableWindow _table;
        private StandardWindow    _registerWindow;

        private CornerIcon     _cornerIcon;
        private AsyncTexture2D _icon;
        private AsyncTexture2D _hoverIcon;

        internal SettingEntry<LfoConfig>   LfoConfig;
        internal SettingEntry<TableConfig> TableConfig;

        protected override void DefineSettings(SettingCollection settings) {
            var selfManaged = settings.AddSubCollection("configs", false, false);
            LfoConfig   = selfManaged.DefineSetting("lfo_config",   Core.UI.Configs.LfoConfig.Default);
            TableConfig = selfManaged.DefineSetting("table_config", Core.UI.Configs.TableConfig.Default);
        }

        protected override void Initialize() {
            Resources = new ResourceService();
            KpWebApi  = new KpWebApiService();
            PartySync = new PartySyncService();
            Gw2WebApi = new Gw2WebApiService();
        }

        protected override async Task LoadAsync() {
            await Resources.LoadAsync();
            await PartySync.InitSquad();
        }

        protected override void OnModuleLoaded(EventArgs e) {
            GameService.ArcDps.Common.Activate();

            _icon = ContentsManager.GetTexture("icon.png");
            _hoverIcon = ContentsManager.GetTexture("hover_icon.png");
            _cornerIcon = new CornerIcon(_icon, _hoverIcon,"Kill Proof") {
                MouseInHouse = true,
                Priority = 236278055 // Arbitrary value that should be unique to this module.
            };

            _window = new TabbedWindow2(GameService.Content.DatAssetCache.GetTextureFromAssetId(155985), 
                                         new Rectangle(40, 26, 913, 691), 
                                         new Rectangle(100, 36, 839, 605)) {
                Parent        = GameService.Graphics.SpriteScreen,
                Title         = this.Name,
                Emblem        = _icon,
                Subtitle      = "Kill Proof",
                Id            = $"{nameof(ProofLogix)}_KillProof_91702dd39f0340b5bd7883cc566e4f63",
                CanResize     = true,
                SavesSize     = true,
                SavesPosition = true,
                Width         = 700,
                Height        = 600,
                Visible = false
            };

            _window.Tabs.Add(new Tab(GameService.Content.DatAssetCache.GetTextureFromAssetId(255369), () => new HomeView(),          "Account"));
            _window.Tabs.Add(new Tab(GameService.Content.DatAssetCache.GetTextureFromAssetId(156680), () => new LfoView(LfoConfig.Value), "Looking for Opener"));

            _window.TabChanged += OnTabChanged;


            _table = new OversizableWindow(GameService.Content.DatAssetCache.GetTextureFromAssetId(155985),
                                        new Rectangle(40, 26, 913, 691),
                                        new Rectangle(70, 36, 839, 605)) 
            {
                Parent             = GameService.Graphics.SpriteScreen,
                Width              = 1000,
                Height             = 500,
                Id                 = $"{nameof(ProofLogix)}_Table_045b4a5441ac40ea93d98ae2021a8f0c",
                Title              = string.Empty, // Prevents Title ("No Title") and Subtitle from being drawn.
                CanResize          = true,
                SavesSize          = true,
                SavesPosition      = true,
                CanCloseWithEscape = false, // Prevents accidental closing as table is treated as part of the HUD.
                Visible            = false
            };
            _cornerIcon.Click += OnCornerIconClick;

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        public void ToggleRegisterWindow() {
            if (_registerWindow != null) {
                _registerWindow.Left = (GameService.Graphics.SpriteScreen.Width - _registerWindow.Width) / 2;
                _registerWindow.Top = (GameService.Graphics.SpriteScreen.Height - _registerWindow.Height) / 2;
                _registerWindow.BringWindowToFront();
                _registerWindow.Show();
                return;
            }
            _registerWindow = new StandardWindow(GameService.Content.DatAssetCache.GetTextureFromAssetId(155985),
                                                 new Rectangle(40, 26, 913, 691),
                                                 new Rectangle(70, 36, 839, 605)) {
                Parent    = GameService.Graphics.SpriteScreen,
                Title     = "Not Yet Registered",
                Subtitle  = "Kill Proof",
                CanResize = false,
                Width     = 700,
                Height    = 550,
                Left      = (GameService.Graphics.SpriteScreen.Width  - 700) / 2,
                Top       = (GameService.Graphics.SpriteScreen.Height - 600) / 2,
                Emblem    = ProofLogix.Instance.ContentsManager.GetTexture("killproof_icon.png")
            };
            _registerWindow.Show(new RegisterView());
        }

        public void ToggleTable() {
            _table.ToggleWindow(new TableView(TableConfig.Value));
        }

        private void OnTabChanged(object sender, ValueChangedEventArgs<Tab> e) {
            if (sender is not TabbedWindow2 wnd) {
                return;
            }
            wnd.Subtitle = e.NewValue.Name;
        }

        private void OnCornerIconClick(object sender, MouseEventArgs e) {
            _window.ToggleWindow();
        }

        /// <inheritdoc />
        protected override void Unload() {
            _window.TabChanged -= OnTabChanged;
            _cornerIcon.Click  -= OnCornerIconClick;
            _cornerIcon?.Dispose();
            _window?.Dispose();
            _registerWindow?.Dispose();
            _table?.Dispose();
            _hoverIcon?.Dispose();
            _icon?.Dispose();

            KpWebApi.Dispose();
            PartySync.Dispose();
            Resources.Dispose();
            Gw2WebApi.Dispose();

            // All static members must be manually unset
            Instance = null;
            TrackableWindow.Unset();
        }
    }
}
