using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Input;
using Nekres.ProofLogix.Core.Services;
using Nekres.ProofLogix.Core.UI;
using Nekres.ProofLogix.Core.UI.Configs;
using Nekres.ProofLogix.Core.UI.Home;
using Nekres.ProofLogix.Core.UI.LookingForOpener;
using Nekres.ProofLogix.Core.UI.SmartPing;
using Nekres.ProofLogix.Core.UI.Table;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Gw2WebApiService = Nekres.ProofLogix.Core.Services.Gw2WebApiService;
using MouseEventArgs = Blish_HUD.Input.MouseEventArgs;
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

        private TabbedWindow2      _window;
        private LockableAxisWindow _table;
        private StandardWindow     _registerWindow;
        private StandardWindow     _smartPing;

        private CornerIcon     _cornerIcon;
        internal AsyncTexture2D Emblem;
        private AsyncTexture2D _icon;
        private AsyncTexture2D _hoverIcon;

        internal SettingEntry<LfoConfig>       LfoConfig;
        internal SettingEntry<TableConfig>     TableConfig;
        internal SettingEntry<SmartPingConfig> SmartPingConfig;
        internal SettingEntry<KeyBinding>      ChatMessageKey;

        private SettingEntry<KeyBinding> _tableKey;
        private SettingEntry<KeyBinding> _smartPingKey;

        protected override void DefineSettings(SettingCollection settings) {
            var keyBindings = settings.AddSubCollection("bindings", true, false, () => "Key Bindings");
            _tableKey = keyBindings.DefineSetting("table_key", new KeyBinding(ModifierKeys.Ctrl, Keys.K), 
                                                () => "Party Table", 
                                                () => "Open or close the Party Table dialog.");
            _smartPingKey = keyBindings.DefineSetting("smart_ping_key", new KeyBinding(ModifierKeys.Ctrl, Keys.L),
                                                      () => "Smart Ping",
                                                      () => "Open or close the Smart Ping dialog.");
            ChatMessageKey = keyBindings.DefineSetting("chat_message_key", new KeyBinding(Keys.Enter),
                                                       () => "Chat Message",
                                                       () => "Give focus to the chat edit box.");

            var selfManaged = settings.AddSubCollection("configs", false, false);
            LfoConfig       = selfManaged.DefineSetting("lfo_config",        Core.UI.Configs.LfoConfig.Default);
            TableConfig     = selfManaged.DefineSetting("table_config",      Core.UI.Configs.TableConfig.Default);
            SmartPingConfig = selfManaged.DefineSetting("smart_ping_config", Core.UI.Configs.SmartPingConfig.Default);
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

            Emblem    = ContentsManager.GetTexture("emblem.png");
            _icon      = ContentsManager.GetTexture("icon.png");
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
                Subtitle      = "Account",
                Emblem        = Emblem,
                Id            = $"{nameof(ProofLogix)}_KillProof_91702dd39f0340b5bd7883cc566e4f63",
                CanResize     = true,
                SavesSize     = true,
                SavesPosition = true,
                Width         = 700,
                Height        = 600,
                Visible       = false
            };

            _window.Tabs.Add(new Tab(GameService.Content.DatAssetCache.GetTextureFromAssetId(255369), () => new HomeView(), "Account"));
            _window.Tabs.Add(new Tab(GameService.Content.DatAssetCache.GetTextureFromAssetId(156680), 
                                     () => Resources.HasLoaded()
                                               ? new LfoView(LfoConfig.Value) 
                                               : new LoadingView("Service unavailable…", "Please, try again later."), 
                                     "Looking for Opener"));

            _window.TabChanged += OnTabChanged;

            _table = new LockableAxisWindow(GameService.Content.DatAssetCache.GetTextureFromAssetId(155985),
                                        new Rectangle(40, 26, 913, 691),
                                        new Rectangle(70, 36, 839, 605)) 
            {
                Parent             = GameService.Graphics.SpriteScreen,
                Width              = 1000,
                Height             = 500,
                Id                 = $"{nameof(ProofLogix)}_Table_045b4a5441ac40ea93d98ae2021a8f0c",
                Title              = "Party Table",
                Subtitle           = GetKeyCombinationString(_tableKey.Value),
                CanResize          = true,
                SavesSize          = true,
                SavesPosition      = true,
                CanCloseWithEscape = false, // Prevents accidental closing as table is treated as part of the HUD.
                Visible            = false,
                Emblem             = Emblem
            };

            _smartPing = new StandardWindow(GameService.Content.DatAssetCache.GetTextureFromAssetId(155985),
                                            new Rectangle(40, 26, 913, 691),
                                            new Rectangle(70, 36, 839, 645)) {
                Parent             = GameService.Graphics.SpriteScreen,
                Width              = 500,
                Height             = 150,
                Id                 = $"{nameof(ProofLogix)}_SmartPing_1f4fa9243b014915bfb7af4be545cb7b",
                Title              = "Smart Ping",
                Subtitle           = GetKeyCombinationString(_smartPingKey.Value),
                SavesPosition      = true,
                CanCloseWithEscape = false,
                Visible            = false,
                Emblem             = Emblem
            };

            _cornerIcon.Click += OnCornerIconClick;

            _tableKey.Value.Activated      += OnTableKeyActivated;
            _tableKey.Value.BindingChanged += OnTableKeyBindingChanged;
            _tableKey.Value.Enabled        =  true;

            _smartPingKey.Value.Activated      += OnSmartPingKeyActivated;              
            _smartPingKey.Value.BindingChanged += OnSmartPingKeyBindingChanged;
            _smartPingKey.Value.Enabled        =  true;
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private void OnSmartPingKeyBindingChanged(object sender, EventArgs e) {
            if (_smartPing != null) {
                _smartPing.Subtitle = GetKeyCombinationString(_smartPingKey.Value);
            }
        }

        private void OnSmartPingKeyActivated(object sender, EventArgs e) {
            ToggleSmartPing();
        }

        private void OnTableKeyBindingChanged(object sender, EventArgs e) {
            if (_table != null) {
                _table.Subtitle = GetKeyCombinationString(_tableKey.Value);
            }
        }

        private void OnTableKeyActivated(object sender, EventArgs e) {
            ToggleTable();
        }

        private string GetKeyCombinationString(KeyBinding keyBinding) {
            if (keyBinding.ModifierKeys == ModifierKeys.None) {
                return keyBinding.PrimaryKey == Keys.None ? string.Empty : $"[{keyBinding.PrimaryKey}]";
            }
            string modifierString = string.Empty;
            if ((keyBinding.ModifierKeys & ModifierKeys.Ctrl) != 0) {
                modifierString += "Ctrl + ";
            }
            if ((keyBinding.ModifierKeys & ModifierKeys.Alt) != 0) {
                modifierString += "Alt + ";
            }
            if ((keyBinding.ModifierKeys & ModifierKeys.Shift) != 0) {
                modifierString += "Shift + ";
            }
            return $"[{modifierString + keyBinding.PrimaryKey}]";
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
            if (!Resources.HasLoaded()) {
                GameService.Content.PlaySoundEffectByName("error");
                return;
            }
            _table.ToggleWindow(new TableView(TableConfig.Value));
        }

        public void ToggleSmartPing() {
            if (!Resources.HasLoaded()) {
                GameService.Content.PlaySoundEffectByName("error");
                return;
            }
            if (!PartySync.LocalPlayer.HasKpProfile) {
                GameService.Content.PlaySoundEffectByName("error");
                ScreenNotification.ShowNotification("Smart Ping unavailable. Profile not yet loaded.", ScreenNotification.NotificationType.Error);
                return;
            }
            if (PartySync.LocalPlayer.KpProfile.IsEmpty) {
                GameService.Content.PlaySoundEffectByName("error");
                ScreenNotification.ShowNotification("Smart Ping unavailable. Profile has no records.", ScreenNotification.NotificationType.Error);
                return;
            }
            _smartPing.ToggleWindow(new SmartPingView(SmartPingConfig.Value));
        }

        private void OnTabChanged(object sender, ValueChangedEventArgs<Tab> e) {
            if (sender is not WindowBase2 wnd) {
                return;
            }
            wnd.Subtitle = e.NewValue.Name;
        }

        private void OnCornerIconClick(object sender, MouseEventArgs e) {
            _window.ToggleWindow();
        }

        /// <inheritdoc />
        protected override void Unload() {
            if (_smartPingKey != null) {
                _smartPingKey.Value.Enabled        =  false;
                _smartPingKey.Value.BindingChanged -= OnSmartPingKeyBindingChanged;
                _smartPingKey.Value.Activated      -= OnSmartPingKeyActivated;
            }

            if (_tableKey != null) {
                _tableKey.Value.Enabled        =  false;
                _tableKey.Value.BindingChanged -= OnTableKeyBindingChanged;
                _tableKey.Value.Activated      -= OnTableKeyActivated;
            }

            if (_window != null) {
                _window.TabChanged -= OnTabChanged;
                _window.Dispose();
            }

            if (_cornerIcon != null) {
                _cornerIcon.Click -= OnCornerIconClick;
                _cornerIcon.Dispose();
            }

            _registerWindow?.Dispose();
            _table?.Dispose();
            _hoverIcon?.Dispose();
            _icon?.Dispose();
            Emblem?.Dispose();
            KpWebApi.Dispose();
            PartySync.Dispose();
            Resources.Dispose();
            Gw2WebApi.Dispose();

            TrackableWindow.Unset();

            // All static members must be manually unset
            Instance = null;
        }
    }
}
