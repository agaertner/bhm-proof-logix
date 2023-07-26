using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Extended;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework.Audio;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using Nekres.ProofLogix.Core.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.Services {
    internal class ResourceService : IDisposable {

        private Dictionary<int, string>         _profNames;
        private Dictionary<int, AsyncTexture2D> _profIcons;
        private Dictionary<int, string>         _eliteNames;
        private Dictionary<int, AsyncTexture2D> _eliteIcons;

        private Resources _resources;

        private Dictionary<int, AsyncTexture2D> _apiIcons;

        public readonly IReadOnlyList<int> ObsoleteItemIds = new List<int> {
            88485, // Legendary Divination
            81743, // Unstable Cosmic Essence
            12251, // Banana
            12773, // Bananas in Bulk
        };

        private IReadOnlyList<SoundEffect> _menuClicks;
        private SoundEffect                _menuItemClickSfx;

        private readonly IReadOnlyList<string> _loadingText = new List<string> {
            "Turning Vault upside down...",
            "Borrowing wallet...",
            "Tickling characters...",
            "High-fiving Deimos...",
            "Checking on Dhuum's cage...",
            "Throwing rocks into Mystic Forge...",
            "Lock-picking Ahdashim...",
            "Mounting Gorseval...",
            "Knitting Xera's ribbon...",
            "Caring for the bees...",
            "Dismantling White Mantle...",
            "Chasing Skritt...",
            "Ransacking bags...",
            "Poking Saul...",
            "Commanding golems...",
            "Polishing monocle...",
            "Running in circles...",
            "Scratching Slothasor...",
            "Cleaning Kitty Golem...",
            "Making sense of inventory...",
            "Pleading for Glenna's assistance...",
            "Counting achievements...",
            "Blowing away dust...",
            "Calling upon spirits...",
            "Consulting Order of Shadows...",
            "Bribing Pact troops...",
            "Bribing Bankers..."
        };

        public ResourceService() {
            LoadSounds();

            _profNames  = new Dictionary<int, string>();
            _profIcons  = new Dictionary<int, AsyncTexture2D>();
            _eliteNames = new Dictionary<int, string>();
            _eliteIcons = new Dictionary<int, AsyncTexture2D>();
            _resources  = Resources.Empty;
            _apiIcons   = new Dictionary<int, AsyncTexture2D>();

            GameService.Overlay.UserLocaleChanged += OnUserLocaleChanged;
        }

        public string GetLoadingSubtitle() {
            return _loadingText[RandomUtil.GetRandom(0, _loadingText.Count - 1)];
        }

        public void PlayMenuItemClick() {
            _menuItemClickSfx.Play(GameService.GameIntegration.Audio.Volume, 0, 0);
        }

        public void PlayMenuClick() {
            _menuClicks[RandomUtil.GetRandom(0, 3)].Play(GameService.GameIntegration.Audio.Volume, 0, 0);
        }

        public async Task LoadAsync(bool localeChange = false) {

            await LoadProfessions(localeChange);
            await LoadResources(localeChange);
        }

        private void LoadSounds() {
            _menuItemClickSfx = ProofLogix.Instance.ContentsManager.GetSound(@"audio\menu-item-click.wav");
            _menuClicks = new List<SoundEffect> {
                ProofLogix.Instance.ContentsManager.GetSound(@"audio\menu-click-1.wav"),
                ProofLogix.Instance.ContentsManager.GetSound(@"audio\menu-click-2.wav"),
                ProofLogix.Instance.ContentsManager.GetSound(@"audio\menu-click-3.wav"),
                ProofLogix.Instance.ContentsManager.GetSound(@"audio\menu-click-4.wav")
            };
        }

        private async Task LoadResources(bool localeChange = false) {
            _resources = await ProofLogix.Instance.KpWebApi.GetResources();

            foreach (var wing in _resources.Wings) {
                wing.Name = await GetMapName(wing.MapId);
            }

            if (!localeChange) {
                foreach (var res in _resources.Items) {
                    _apiIcons.Add(res.Id, res.Icon);
                }
            }
        }

        public List<Token> FilterObsoleteItems(IEnumerable<Token> tokens) {
            return tokens.Where(token => ObsoleteItemIds.All(id => id != token.Id)).ToList();
        }

        public string GetClassName(int profession, int elite) {
            return _eliteNames.TryGetValue(elite, out var name) ? name :
                   _profNames.TryGetValue(profession, out name) ? name : string.Empty;
        }

        public AsyncTexture2D GetClassIcon(int profession, int elite) {
            return _eliteIcons.TryGetValue(elite, out var icon) ? icon :
                   _profIcons.TryGetValue(profession, out icon) ? icon : ContentService.Textures.TransparentPixel;
        }

        public Resource GetResource(int id) {
            return _resources.Items.FirstOrDefault(item => item.Id == id);
        }

        public List<Raid.Wing> GetWings() {
            return _resources.Wings.ToList();
        }

        public List<Resource> GetResourcesForMap(int mapId) {
            if (_resources.IsEmpty) {
                return Enumerable.Empty<Resource>().ToList();
            }

            var items = _resources.Raids.SelectMany(x => x.Wings)
                                   .Where(x => x.MapId == mapId)
                                   .SelectMany(x => x.Events)
                                   .SelectMany(x => x.GetTokens());

            return items.ToList();
        }

        public List<Resource> GetItemsForFractals(bool includeOld = false) {
            var fractalItems = _resources.IsEmpty ? Enumerable.Empty<Resource>() : _resources.Fractals;
            if (!includeOld) {
                fractalItems = fractalItems.Where(item => !ObsoleteItemIds.Contains(item.Id));
            }
            return fractalItems.ToList();
        }

        public List<Resource> GetGeneralItems(bool includeOld = false) {
            var generalItems = _resources.IsEmpty ? Enumerable.Empty<Resource>() : _resources.GeneralTokens;
            if (!includeOld) {
                generalItems = generalItems.Where(item => !ObsoleteItemIds.Contains(item.Id));
            }
            return generalItems.ToList();
        }

        public AsyncTexture2D GetApiIcon(int itemId) {
            if (_apiIcons.TryGetValue(itemId, out var tex)) {
                return tex;
            }

            var response = GameService.Gw2WebApi.AnonymousConnection.Client.V2.Items.GetAsync(itemId).Result;

            if (response?.Icon == null) {
                return ContentService.Textures.TransparentPixel;
            }

            var assetId = AssetUtil.GetId(response.Icon);
            var icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(assetId);
            _apiIcons.Add(itemId, icon);
            return icon;
        }

        public async Task<string> GetMapName(int mapId) {
            var map = await HttpUtil.RetryAsync(() => ProofLogix.Instance.Gw2ApiManager.Gw2ApiClient.V2.Maps.GetAsync(mapId));
            return map?.Name ?? string.Empty;
        }

        public void Dispose() {
            GameService.Overlay.UserLocaleChanged -= OnUserLocaleChanged;

            _menuItemClickSfx.Dispose();
            foreach (var sfx in _menuClicks) {
                sfx.Dispose();
            }
        }

        private async void OnUserLocaleChanged(object sender, ValueEventArgs<CultureInfo> e) {
            await LoadAsync(true);
        }

        private async Task LoadProfessions(bool localeChange = false) {
            var professions = await TaskUtil.RetryAsync(() => GameService.Gw2WebApi.AnonymousConnection.Client.V2.Professions.AllAsync());

            if (professions == null) {
                return;
            }

            var specializations = await TaskUtil.RetryAsync(() => GameService.Gw2WebApi.AnonymousConnection.Client.V2.Specializations.AllAsync());

            if (specializations == null) {
                return;
            }

            var elites = specializations.Where(x => x.Elite).ToList();

            _profNames  = professions.ToDictionary(x => (int)(ProfessionType)Enum.Parse(typeof(ProfessionType), x.Id, true), x => x.Name);
            _eliteNames = elites.ToDictionary(x => x.Id, x => x.Name);

            if (localeChange) {
                return;
            }

            _profIcons  = professions.ToDictionary(x => (int)(ProfessionType)Enum.Parse(typeof(ProfessionType), x.Id, true), x => GameService.Content.GetRenderServiceTexture(x.IconBig.ToString()));
            _eliteIcons = elites.ToDictionary(x => x.Id, x => GameService.Content.GetRenderServiceTexture(x.ProfessionIconBig.ToString()));
        }

        public List<Resource> GetItems() {
            return _resources.Items.ToList();
        }

        public List<Raid> GetRaids() {
            return _resources.Raids;
        }

    }
}
