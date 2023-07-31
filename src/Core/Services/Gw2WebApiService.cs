﻿using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Extended;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.Services {
    internal class Gw2WebApiService : IDisposable {

        //private const int UPDATE_INTERVAL_MIN = 5; // Should reflect GW2 API update interval.

        //private DateTime _lastUpdate = DateTime.UtcNow.AddMinutes(-UPDATE_INTERVAL_MIN);

        private readonly IReadOnlyList<TokenPermission> _requires = new List<TokenPermission> {
            TokenPermission.Account,
            TokenPermission.Progression,
            TokenPermission.Inventories,
            TokenPermission.Characters
        };

        public bool HasSubtoken;

        public IReadOnlyList<TokenPermission> MissingPermissions;

        private Regex _apiKeyPattern = new(@"^[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{20}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}$");

        public Gw2WebApiService() {
            MissingPermissions = new List<TokenPermission>();

            ProofLogix.Instance.Gw2ApiManager.SubtokenUpdated += OnSubtokenUpdated;
        }

        public void Dispose() {
            ProofLogix.Instance.Gw2ApiManager.SubtokenUpdated -= OnSubtokenUpdated;
        }

        public bool IsApiAvailable() {
            if (string.IsNullOrWhiteSpace(GameService.Gw2Mumble.PlayerCharacter.Name)) {
                ScreenNotification.ShowNotification("API unavailable. Please, login to a character.", ScreenNotification.NotificationType.Error);
                return false;
            }

            if (!HasSubtoken) {
                ScreenNotification.ShowNotification("Missing API key. Please, add an API key to BlishHUD.", ScreenNotification.NotificationType.Error);
                return false;
            }

            if (MissingPermissions.Any()) {
                var missing = string.Join(", ", ProofLogix.Instance.Gw2WebApi.MissingPermissions);
                ScreenNotification.ShowNotification($"Insufficient API permissions.\nRequired: {missing}", ScreenNotification.NotificationType.Error);
                return false;
            }
            return true;
        }

        public async Task<List<string>> GetClears() {
            var clears = await TaskUtil.RetryAsync(() => ProofLogix.Instance.Gw2ApiManager.Gw2ApiClient.V2.Account.Raids.GetAsync());
            return (clears ?? Enumerable.Empty<string>()).ToList();
        }

        public async Task<List<AccountItem>> GetBank() {
            var bank = await TaskUtil.RetryAsync(() => ProofLogix.Instance.Gw2ApiManager.Gw2ApiClient.V2.Account.Bank.GetAsync());
            return FilterProofs(bank).ToList();
        }

        public async Task<List<AccountItem>> GetSharedBags() {
            var sharedBags = await TaskUtil.RetryAsync(() => ProofLogix.Instance.Gw2ApiManager.Gw2ApiClient.V2.Account.Inventory.GetAsync());
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

        public bool HasCorrectFormat(string apiKey) {
            return !string.IsNullOrWhiteSpace(apiKey) && _apiKeyPattern.IsMatch(apiKey);
        }

        private async Task<IEnumerable<Character>> GetCharacters() {
            var characters = await TaskUtil.RetryAsync(() => ProofLogix.Instance.Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync());
            return characters ?? Enumerable.Empty<Character>();
        }

        private void OnSubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e) {
            HasSubtoken        = true;
            MissingPermissions = e.Value.Intersect(_requires).Except(_requires).ToList();
        }

        private IEnumerable<AccountItem> FilterProofs(IEnumerable<AccountItem> items) {
            var resources = ProofLogix.Instance.Resources.GetItems();
            return items?.Where(item => item != null && resources.Select(res => res.Id).Contains(item.Id))
                ?? Enumerable.Empty<AccountItem>();
        }
    }
}
