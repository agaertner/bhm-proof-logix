using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Extended;
using Gw2Sharp.Models;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.Services {
    internal class ResourceService : IDisposable {

        private static Dictionary<int, string>         _profNames  = new();
        private static Dictionary<int, AsyncTexture2D> _profIcons  = new();
        private static Dictionary<int, string>         _eliteNames = new();
        private static Dictionary<int, AsyncTexture2D> _eliteIcons = new();

        private static Resources _resources = Resources.Empty;

        private static IReadOnlyList<int> _outdatedItemIds = new List<int>() {
            81743,
            88485
        };

        public ResourceService() {
            GameService.Overlay.UserLocaleChanged += OnUserLocaleChanged;
        }

        public async Task LoadAsync(bool localeChange = false) {
            await LoadProfessions(localeChange);
            await LoadResources();
        }

        private async Task LoadResources() {
            _resources  = await ProofLogix.Instance.KpWebApi.GetResources();
        }

        public static string GetClassName(int profession, int elite) {
            return _eliteNames.TryGetValue(elite, out var name) ? name :
                   _profNames.TryGetValue(profession, out name) ? name : string.Empty;
        }

        public static AsyncTexture2D GetClassIcon(int profession, int elite) {
            return _eliteIcons.TryGetValue(elite, out var icon) ? icon :
                   _profIcons.TryGetValue(profession, out icon) ? icon : ContentService.Textures.TransparentPixel;
        }

        public static Resource GetItem(int id) {
            return _resources.Items.FirstOrDefault(item => item.Id == id);
        }

        public static List<Raid.Wing> GetWings() {
            return _resources.Wings.ToList();
        }

        public static List<Resource> GetItemsForMap(int mapId) {
            if (_resources.IsEmpty) {
                return Enumerable.Empty<Resource>().ToList();
            }

            var items = _resources.Raids.SelectMany(x => x.Wings)
                                   .Where(x => x.MapId == mapId)
                                   .SelectMany(x => x.Events)
                                   .SelectMany(x => x.GetTokens());

            return items.ToList();
        }

        public static List<Resource> GetItemsForFractals(bool includeOld = false) {
            var fractalItems = _resources.IsEmpty ? Enumerable.Empty<Resource>() : _resources.Fractals;
            if (!includeOld) {
                fractalItems = fractalItems.Where(item => !_outdatedItemIds.Contains(item.Id));
            }
            return fractalItems.ToList();
        }

        public static List<Resource> GetGeneralItems(bool includeOld = false) {
            var generalItems = _resources.IsEmpty ? Enumerable.Empty<Resource>() : _resources.GeneralTokens;
            if (!includeOld) {
                generalItems = generalItems.Where(item => !_outdatedItemIds.Contains(item.Id));
            }
            return generalItems.ToList();
        }

        public void Dispose() {
            GameService.Overlay.UserLocaleChanged -= OnUserLocaleChanged;

            _eliteNames      = null;
            _profNames       = null;
            _eliteIcons      = null;
            _profIcons       = null;
            _resources       = null;
            _outdatedItemIds = null;
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
    }
}
