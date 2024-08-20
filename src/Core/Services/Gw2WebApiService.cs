using Blish_HUD;
using Blish_HUD.Extended;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.Services {
    internal class Gw2WebApiService : IDisposable {

        public readonly TokenPermission[] Requires = {
            TokenPermission.Account,
            TokenPermission.Progression,
            TokenPermission.Inventories,
            TokenPermission.Characters,
            TokenPermission.Builds
        };

        public Gw2WebApiService() {
            /* NOOP */
        }

        public void Dispose() {
            /* NOOP */
        }

        public async Task<List<string>> GetClears() {
            var clears = await TaskUtil.TryAsync(() => ProofLogix.Instance.Gw2ApiManager.Gw2ApiClient.V2.Account.Raids.GetAsync());
            return (clears ?? Enumerable.Empty<string>()).ToList();
        }

        public async Task<List<AccountItem>> GetBank() {
            var bank = await TaskUtil.TryAsync(() => ProofLogix.Instance.Gw2ApiManager.Gw2ApiClient.V2.Account.Bank.GetAsync());
            return FilterProofs(bank).ToList();
        }

        public async Task<List<AccountItem>> GetSharedBags() {
            var sharedBags = await TaskUtil.TryAsync(() => ProofLogix.Instance.Gw2ApiManager.Gw2ApiClient.V2.Account.Inventory.GetAsync());
            return FilterProofs(sharedBags).ToList();
        }

        public async Task<Dictionary<Character, List<AccountItem>>> GetBagsByCharacter() {
            var characters = await GetCharacters();

            var bagsByCharacter = new Dictionary<Character, List<AccountItem>>();

            foreach (var character in characters) {

                if (character.Bags == null) {
                    continue;
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                var bags = character.Bags.Where(bag => bag != null); // bag slots can be empty.

                var filtered = FilterProofs(bags.SelectMany(bag => bag.Inventory)).ToList();

                if (!filtered.Any()) {
                    continue;
                }

                bagsByCharacter.Add(character, filtered);
            }

            return bagsByCharacter;
        }

        public async Task<IReadOnlyList<Item>> GetItems(params int[] itemIds) {
            var response = await TaskUtil.TryAsync(() => GameService.Gw2WebApi.AnonymousConnection.Client.V2.Items.ManyAsync(itemIds));
            return response ?? Enumerable.Empty<Item>().ToList();
        }

        public async Task<IReadOnlyList<Map>> GetMaps(params int[] mapIds) {
            var response = await TaskUtil.TryAsync(() => ProofLogix.Instance.Gw2ApiManager.Gw2ApiClient.V2.Maps.ManyAsync(mapIds));
            return response ?? Enumerable.Empty<Map>().ToList();
        }

        private async Task<IEnumerable<Character>> GetCharacters() {
            var characters = await TaskUtil.TryAsync(() => ProofLogix.Instance.Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync());
            return characters ?? Enumerable.Empty<Character>();
        }

        private IEnumerable<AccountItem> FilterProofs(IEnumerable<AccountItem> items) {
            var resources = ProofLogix.Instance.Resources.GetItems();
            return items?.Where(item => item != null && resources.Select(res => res.Id).Contains(item.Id))
                ?? Enumerable.Empty<AccountItem>();
        }
    }
}
