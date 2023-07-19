using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Extended;
using Gw2Sharp.Models;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using Nekres.ProofLogix.Core.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Raid = Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models.Raid;

namespace Nekres.ProofLogix.Core.Services {
    internal class ResourceService : IDisposable {

        private Dictionary<int, string>         _profNames;
        private Dictionary<int, AsyncTexture2D> _profIcons;
        private Dictionary<int, string>         _eliteNames;
        private Dictionary<int, AsyncTexture2D> _eliteIcons;

        private Resources _resources;

        private Dictionary<int, AsyncTexture2D> _apiIcons;

        public IReadOnlyList<int> ObsoleteItemIds;

        public ResourceService() {
            _profNames  = new Dictionary<int, string>();
            _profIcons  = new Dictionary<int, AsyncTexture2D>();
            _eliteNames = new Dictionary<int, string>();
            _eliteIcons = new Dictionary<int, AsyncTexture2D>();
            _resources  = Resources.Empty;
            _apiIcons   = new Dictionary<int, AsyncTexture2D>();
            this.ObsoleteItemIds = new List<int> {
                88485, // Legendary Divination
                81743, // Unstable Cosmic Essence
                12251, // Banana
                12773, // Bananas in Bulk
            };

            GameService.Overlay.UserLocaleChanged += OnUserLocaleChanged;
        }

        public async Task LoadAsync(bool localeChange = false) {
            await LoadProfessions(localeChange);
            await LoadResources();
        }

        private async Task LoadResources() {
            _resources = await ProofLogix.Instance.KpWebApi.GetResources();

            foreach (var wing in _resources.Wings) {
                wing.Name = await GetMapName(wing.MapId);
            }
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
