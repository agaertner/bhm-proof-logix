﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Extended;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
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

        private IReadOnlyList<Map> _maps;

        private Dictionary<int, AsyncTexture2D> _apiIcons;

        private readonly IReadOnlyList<string> _loadingText = new List<string> {
            "Turning Vault upside down…",
            "Borrowing wallet…",
            "Tickling characters…",
            "High-fiving Deimos…",
            "Checking on Dhuum's cage…",
            "Throwing rocks into Mystic Forge…",
            "Lock-picking Ahdashim…",
            "Mounting Gorseval…",
            "Knitting Xera's ribbon…",
            "Caring for the bees…",
            "Dismantling White Mantle…",
            "Chasing Skritt…",
            "Ransacking bags…",
            "Poking Saul…",
            "Commanding golems…",
            "Polishing monocle…",
            "Running in circles…",
            "Scratching Slothasor…",
            "Cleaning Kitty Golem…",
            "Making sense of inventory…",
            "Pleading for Glenna's assistance…",
            "Counting achievements…",
            "Blowing away dust…",
            "Calling upon spirits…",
            "Consulting Order of Shadows…",
            "Bribing Pact troops…",
            "Bribing Bankers…"
        };

        private const string RESOURCE_PROFILE = "Nika"; // Used to add resources missing from the resources endpoint.

        public ResourceService() {
            _resources  = Resources.Empty;
            _profNames  = new Dictionary<int, string>();
            _profIcons  = new Dictionary<int, AsyncTexture2D>();
            _eliteNames = new Dictionary<int, string>();
            _eliteIcons = new Dictionary<int, AsyncTexture2D>();
            _apiIcons   = new Dictionary<int, AsyncTexture2D>();

            GameService.Overlay.UserLocaleChanged += OnUserLocaleChanged;
        }

        public bool HasLoaded() {
            if (_resources.IsEmpty) {
                ScreenNotification.ShowNotification("Unavailable. Resources not yet loaded.", ScreenNotification.NotificationType.Error);
                return false;
            }
            return true;
        }

        public async Task LoadAsync(bool localeChange = false) {
            await LoadProfessions(localeChange);
            await LoadResources();
            await LoadApiIcons(_resources.Items.Select(item => item.Id));
        }

        public string GetLoadingSubtitle() {
            return _loadingText[RandomUtil.GetRandom(0, _loadingText.Count - 1)];
        }

        public void AddNewCoffers(Profile profile) {
            if (_resources.IsEmpty || profile.IsEmpty) {
                return;
            }
            var totals = (profile.Totals.Tokens ?? Enumerable.Empty<Token>()).Where(x => x.Id != Resources.BANANAS
                                                                                     && x.Id != Resources.BANANAS_IN_BULK);

            // Strike and raid tokens are mixed under tokens in the response.
            // Split Strike tokens in the profile from raid tokens by comparing with the resources response which just includes raid tokens.
            var raidResources = GetItemsForRaids();
            var strikeResources = totals.Where(token => raidResources.All(x => x.Id != token.Id))
                                    .Select(token => new Resource {
                                         Id   = token.Id,
                                         Name = token.Name
                                     });
            _resources.Strikes.AddRange(strikeResources); // Add strike tokens to its own list.

            // Strike and Raid coffers.
            var coffers = profile.Totals.Coffers ?? Enumerable.Empty<Token>();
            var newCoffers = coffers.Where(token => _resources.Items.All(x => x.Id != token.Id))
                                    .Select(token => new Resource {
                                         Id   = token.Id,
                                         Name = token.Name
                                     });
            _resources.Coffers.AddRange(newCoffers); // Add missing coffers.
        }

        private async Task LoadResources() {
            do { // Locale change requires the request to be made at least once even if not empty.
                _resources = await ProofLogix.Instance.KpWebApi.GetResources();

                if (_resources.IsEmpty) {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
            } while (_resources.IsEmpty);
            
            _maps = await ProofLogix.Instance.Gw2WebApi.GetMaps(_resources.Wings.Select(wing => wing.MapId).ToArray());

            AddNewCoffers(await ProofLogix.Instance.KpWebApi.GetProfile(RESOURCE_PROFILE));
            await ExpandResources(_resources.Items);
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

            tex = new AsyncTexture2D();
            _apiIcons.Add(itemId, tex);
            return tex;


        }

        private async Task LoadApiIcons(IEnumerable<int> itemIds) {
            var response = await ProofLogix.Instance.Gw2WebApi.GetItems(itemIds.ToArray());

            if (response == null || !response.Any()) {
                return;
            }

            _apiIcons = response.ToDictionary(item => item.Id, 
                                              item => GameService.Content.DatAssetCache.GetTextureFromAssetId(AssetUtil.GetId(item.Icon)));
        }

        public string GetMapName(int mapId) {
            return _maps.FirstOrDefault(map => map.Id == mapId)?.Name ?? string.Empty;
        }

        public void Dispose() {
            GameService.Overlay.UserLocaleChanged -= OnUserLocaleChanged;
        }

        private async Task ExpandResources(IEnumerable<Resource> resources) {
            var items = await ProofLogix.Instance.Gw2WebApi.GetItems(resources.Select(resource => resource.Id).ToArray());

            if (items == null || !items.Any()) {
                return;
            }

            foreach (var item in items) {
                var resource = _resources.Items.FirstOrDefault(resource => resource.Id == item.Id);

                if (resource != null) {
                    resource.Rarity = item.Rarity;
                    resource.Icon   = GameService.Content.DatAssetCache.GetTextureFromAssetId(AssetUtil.GetId(item.Icon));
                    resource.Name = item.Name;
                }
            }
        }

        private async void OnUserLocaleChanged(object sender, ValueEventArgs<CultureInfo> e) {
            await LoadAsync(true);
        }

        private async Task LoadProfessions(bool localeChange = false) {
            var professions = await TaskUtil.TryAsync(() => GameService.Gw2WebApi.AnonymousConnection.Client.V2.Professions.AllAsync());

            if (professions == null) {
                return;
            }

            var specializations = await TaskUtil.TryAsync(() => GameService.Gw2WebApi.AnonymousConnection.Client.V2.Specializations.AllAsync());

            if (specializations == null) {
                return;
            }

            var elites = specializations.Where(x => x.Elite).ToList();

            _profNames  = professions.ToDictionary(x => (int)(ProfessionType)Enum.Parse(typeof(ProfessionType), x.Id, true), x => x.Name);
            _eliteNames = elites.ToDictionary(x => x.Id, x => x.Name);

            if (localeChange) {
                return;
            }

            _profIcons  = professions.ToDictionary(x => (int)(ProfessionType)Enum.Parse(typeof(ProfessionType), x.Id, true), x => GameService.Content.DatAssetCache.GetTextureFromAssetId(AssetUtil.GetId(x.IconBig)));
            _eliteIcons = elites.ToDictionary(x => x.Id, x => GameService.Content.DatAssetCache.GetTextureFromAssetId(AssetUtil.GetId(x.ProfessionIconBig)));
        }

        public Resource GetItem(int id) {
            return _resources.Items.FirstOrDefault(item => item.Id == id) ?? Resource.Empty;
        }

        public List<Resource> GetItems(params int[] ids) {
            if (ids == null || !ids.Any()) {
                return _resources.Items.ToList();
            }
            return _resources.Items.Where(x => ids.Contains(x.Id)).ToList();
        }

        public List<Resource> GetItemsForFractals() {
            var fractalItems = _resources.IsEmpty ? Enumerable.Empty<Resource>() : _resources.Fractals;
            return fractalItems.ToList();
        }

        public List<Resource> GetItemsForStrikes() {
            var strikeItems = _resources.IsEmpty ? Enumerable.Empty<Resource>() : _resources.Strikes;
            return strikeItems.ToList();
        }

        public List<Resource> GetItemsForRaids() {
            var items = _resources.Raids.SelectMany(x => x.Wings)
                                  .SelectMany(x => x.Events)
                                  .SelectMany(x => x.GetTokens());
            var strikeResources = GetItemsForStrikes();
            var coffers         = _resources.Coffers.Where(coffer => strikeResources.All(strikeCoffer => strikeCoffer.Id != coffer.Id));
            return items.Concat(coffers).ToList();
        }

        public List<Resource> GetGeneralItems() {
            return _resources.IsEmpty ? Enumerable.Empty<Resource>().ToList() : _resources.GeneralTokens;
        }

        public List<Resource> GetCofferItems() {
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
