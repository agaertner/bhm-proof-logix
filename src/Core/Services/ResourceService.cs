using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Extended;
using Gw2Sharp.Models;
using Nekres.ProofLogix.Core.Services.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.Services {
    internal class ResourceService : IDisposable {
        public static Dictionary<int, string>         ProfNames  { get; private set; }
        public static Dictionary<int, AsyncTexture2D> ProfIcons  { get; private set; }
        public static Dictionary<int, string>         EliteNames { get; private set; }
        public static Dictionary<int, AsyncTexture2D> EliteIcons { get; private set; }
        public static Dictionary<int, string>         ItemNames  { get; private set; }
        public static Dictionary<int, AsyncTexture2D> ItemIcons  { get; private set; }

        public ResourceService() {
            GameService.Overlay.UserLocaleChanged += OnUserLocaleChanged;
        }

        private async void OnUserLocaleChanged(object sender, ValueEventArgs<CultureInfo> e) {
            await LoadAsync(true);
        }

        public async Task LoadAsync(bool localeChange = false) {
            await LoadProfessions(localeChange);
            await LoadItems(localeChange);
        }

        private async Task LoadItems(bool localeChange = false) {

            var ids = Enum.GetValues(typeof(Item)).Cast<int>();
            var items = await TaskUtil.RetryAsync(() => GameService.Gw2WebApi.AnonymousConnection.Client.V2.Items.ManyAsync(ids));

            if (items == null) {
                return;
            }

            ItemNames = items.ToDictionary(x => x.Id, x => x.Name);

            if (localeChange) {
                return;
            }

            ItemIcons = items.ToDictionary(x => x.Id, x => GameService.Content.GetRenderServiceTexture(x.Icon.ToString()));
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

            ProfNames  = professions.ToDictionary(x => (int)(ProfessionType)Enum.Parse(typeof(ProfessionType), x.Id, true), x => x.Name);
            EliteNames = elites.ToDictionary(x => x.Id, x => x.Name);

            if (localeChange) {
                return;
            }

            ProfIcons  = professions.ToDictionary(x => (int)(ProfessionType)Enum.Parse(typeof(ProfessionType), x.Id, true), x => GameService.Content.GetRenderServiceTexture(x.IconBig.ToString()));
            EliteIcons = elites.ToDictionary(x => x.Id, x => GameService.Content.GetRenderServiceTexture(x.ProfessionIconBig.ToString()));
        }

        public void Dispose() {
            GameService.Overlay.UserLocaleChanged -= OnUserLocaleChanged;

            EliteNames = null;
            ProfNames  = null;
            ItemNames  = null;
            EliteIcons = null;
            ProfIcons  = null;
            ItemIcons  = null;
        }
    }
}
