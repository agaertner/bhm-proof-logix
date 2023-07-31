using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Extended;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework.Audio;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
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

        public void AddNewResources(Profile profile) {
            if (_resources.IsEmpty || profile.IsEmpty) {
                return;
            }

            var coffers = profile.Totals.Coffers ?? Enumerable.Empty<Token>();
            foreach (var token in coffers.Where(token => _resources.Items.All(x => x.Id != token.Id))) {
                _resources.Coffers.Add(new Resource {
                    Id   = token.Id,
                    Name = token.Name
                });
            }

            var tokens = profile.Totals.GetTokens(excludeCoffers: true);
            foreach (var token in tokens.Where(token => _resources.Items.All(x => x.Id != token.Id))) {
                _resources.GeneralTokens.Add(new Resource {
                    Id   = token.Id,
                    Name = token.Name
                });
            }
        }

        public async Task LoadAsync(bool localeChange = false) {
            await LoadProfessions(localeChange);
            await LoadResources();
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

        private async Task LoadResources() {
            _resources = await ProofLogix.Instance.KpWebApi.GetResources();
            foreach (var wing in _resources.Wings) {
                wing.Name = await GetMapName(wing.MapId);
            }

            AddNewResources(await ProofLogix.Instance.KpWebApi.GetProfile("Nika"));
        }

        public string GetClassName(int profession, int elite) {
            return _eliteNames.TryGetValue(elite, out var name) ? name :
                   _profNames.TryGetValue(profession, out name) ? name : string.Empty;
        }

        public AsyncTexture2D GetClassIcon(int profession, int elite) {
            return _eliteIcons.TryGetValue(elite, out var icon) ? icon :
                   _profIcons.TryGetValue(profession, out icon) ? icon : ContentService.Textures.TransparentPixel;
        }

        public AsyncTexture2D GetApiIcon(int itemId) {
            if (itemId == 0) {
                return ContentService.Textures.TransparentPixel;
            }

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
            var map = await TaskUtil.RetryAsync(() => ProofLogix.Instance.Gw2ApiManager.Gw2ApiClient.V2.Maps.GetAsync(mapId));
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

        public Resource GetItem(int id) {
            return _resources.Items.FirstOrDefault(item => item.Id == id) ?? Resource.Empty;
        }

        public List<Resource> GetItems() {
            return _resources.Items.ToList();
        }

        public List<Resource> GetItemsForFractals() {
            var fractalItems = _resources.IsEmpty ? Enumerable.Empty<Resource>() : _resources.Fractals;
            return fractalItems.ToList();
        }

        public List<Resource> GetGeneralItems(bool includeOld = false) {
            var generalItems = _resources.IsEmpty ? Enumerable.Empty<Resource>() : _resources.GeneralTokens;
            return generalItems.ToList();
        }

        public List<Resource> GetCofferItems(bool includeOld = false) {
            var cofferItems = _resources.IsEmpty ? Enumerable.Empty<Resource>() : _resources.Coffers;
            return cofferItems.ToList();
        }

        public List<Resource> GetItemsForMap(int mapId) {
            if (_resources.IsEmpty) {
                return Enumerable.Empty<Resource>().ToList();
            }

            var items = _resources.Raids.SelectMany(x => x.Wings)
                                  .Where(x => x.MapId == mapId)
                                  .SelectMany(x => x.Events)
                                  .SelectMany(x => x.GetTokens());

            return items.ToList();
        }

        public List<Raid.Wing> GetWings() {
            return _resources.Wings.ToList();
        }

        public List<Raid> GetRaids() {
            return _resources.Raids;
        }
    }
}
