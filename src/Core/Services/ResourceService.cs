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
        private static Dictionary<int, string>         _itemNames  = new();
        private static Dictionary<int, AsyncTexture2D> _itemIcons  = new();

        private static Resources _resources = Resources.Empty;

        public ResourceService() {
            GameService.Overlay.UserLocaleChanged += OnUserLocaleChanged;
        }

        public async Task LoadAsync(bool localeChange = false) {
            await LoadProfessions(localeChange);
            await LoadItems(localeChange);
        }

        public static string GetClassName(int profession, int elite) {
            return _eliteNames.TryGetValue(elite, out var name) ? name :
                   _profNames.TryGetValue(profession, out name) ? name : string.Empty;
        }

        public static AsyncTexture2D GetClassIcon(int profession, int elite) {
            return _eliteIcons.TryGetValue(elite, out var icon) ? icon :
                   _profIcons.TryGetValue(profession, out icon) ? icon : ContentService.Textures.TransparentPixel;
        }

        public static string GetItemName(int id) {
            return _itemNames.TryGetValue(id, out var name) ? name : string.Empty;
        }

        public static AsyncTexture2D GetItemIcon(int id) {
            return _itemIcons.TryGetValue(id, out var icon) ? icon : ContentService.Textures.TransparentPixel;
        }

        public static List<int> GetItemIds() {
            return _itemNames.Keys.ToList();
        }

        public static List<int> GetItemIdsForMap(int mapId) {
            if (_resources.IsEmpty) {
                return Enumerable.Empty<int>().ToList();
            }

            var items = _resources.Raids.SelectMany(x => x.Wings)
                                   .Where(x => x.MapId == mapId)
                                   .SelectMany(x => x.Events)
                                   .SelectMany(x => x.GetTokens());

            return items.Select(x => x.Id).ToList();
        }

        public void Dispose() {
            GameService.Overlay.UserLocaleChanged -= OnUserLocaleChanged;

            _eliteNames = null;
            _profNames  = null;
            _itemNames  = null;
            _eliteIcons = null;
            _profIcons  = null;
            _itemIcons  = null;
        }

        private async void OnUserLocaleChanged(object sender, ValueEventArgs<CultureInfo> e) {
            await LoadAsync(true);
        }

        private async Task LoadItems(bool localeChange = false) {

            var resources = await ProofLogix.Instance.KpWebApi.GetResources();

            if (resources.IsEmpty) {
                return;
            }

            var items = resources.Raids
                                 .SelectMany(raid => raid.Wings)
                                 .SelectMany(wing => wing.Events)
                                 .SelectMany(ev => ev.GetTokens())
                                 .Concat(resources.Fractals)
                                 .Concat(resources.GeneralTokens)
                                 .GroupBy(resource => resource.Id)
                                 .Select(group => group.First())
                                 .ToList();

            _itemNames = items.ToDictionary(x => x.Id, x => x.Name);

            _resources = resources;

            if (localeChange) {
                return;
            }

            _itemIcons = items.ToDictionary(x => x.Id, x => GameService.Content.GetRenderServiceTexture(x.Icon));
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
