using Blish_HUD;
using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models {

    // Path: /api/resources?lang={code}

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Resources {

        public const int UNSTABLE_COSMIC_ESSENCE = 81743; // Unobtainable since September 15, 2020. Hardcoded to handle original_uce.
        public const int LEGENDARY_DIVINATION    = 88485; // Unobtainable since July 19, 2022. Hardcoded to exclude them from anything but Raids.
        public const int BONESKINNER_RITUAL_VIAL = 93781; // Misplaced in general tokens. Hardcoded to exclude them from anything but Strikes.
        public const int LEGENDARY_INSIGHT       = 77302; // Sometimes we want them in general tokens, sometimes we want them in Raids and Strikes.
        public const int BANANAS_IN_BULK         = 12773; // Hardcoded to move them to general tokens.
        public const int BANANAS                 = 12251; // Hardcoded to move them to general tokens.

        public static Resources Empty = new() {
            IsEmpty = true
        };

        [JsonIgnore]
        public bool IsEmpty { get; private init; }

        [JsonProperty("general_tokens")]
        public List<Resource> GeneralTokens { get; set; }

        [JsonProperty("fractals")]
        public List<Resource> Fractals { get; set; }

        [JsonProperty("raids")]
        public List<Raid> Raids { get; set; }

        [JsonProperty("coffers")]
        public List<Resource> Coffers { get; set; } // Retroactively adding coffers from profile responses.

        [JsonIgnore]
        public List<Resource> Strikes { get; set; } // Retroactively adding strike tokens from profile responses.

        public IEnumerable<Raid.Wing> Wings => this.Raids.SelectMany(raid => raid.Wings);

        public IEnumerable<Resource> Items => this.Raids
                                                  .SelectMany(raid => raid.Wings)
                                                  .SelectMany(wing => wing.Events)
                                                  .SelectMany(ev => ev.GetTokens())
                                                  .Concat(this.Fractals)
                                                  .Concat(this.GeneralTokens)
                                                  .Concat(this.Coffers)
                                                  .Concat(this.Strikes)
                                                  .GroupBy(resource => resource.Id)
                                                  .Select(group => group.First());

        public Resources() {
            this.Coffers = new List<Resource>();
            this.Raids   = new List<Raid>();
            this.Fractals = new List<Resource>();
            this.GeneralTokens = new List<Resource>();
            this.Strikes = new List<Resource>();
            LoadDefaults();
        }

        private void LoadDefaults() {
            this.GeneralTokens.AddRange(new[] { 
                new Resource { Id = BANANAS }, 
                new Resource { Id = BANANAS_IN_BULK }
            });
        }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Resource {

        public static Resource Empty = new() {
            IconUrl = string.Empty,
            Name = string.Empty
        };

        [JsonProperty("icon")]
        public string IconUrl { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonIgnore]
        public ItemRarity Rarity { get; set; }

        [JsonIgnore]
        public AsyncTexture2D Icon { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Raid {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("wings")]
        public List<Wing> Wings { get; set; }

        public Raid() {
            this.Wings = new List<Wing>();
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public sealed class Wing {


            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("map_id")]
            public int MapId { get; set; }

            [JsonProperty("events")]
            public List<Event> Events { get; set; }

            public Wing() {
                this.Events = new List<Event>();
            }

            [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
            public sealed class Event {

                public enum EventType {
                    Boss,
                    Checkpoint
                }

                [JsonProperty("id")]
                public string Id { get; set; }

                [JsonProperty("type")]
                [JsonConverter(typeof(StringEnumConverter))]
                public EventType Type { get; set; }

                [JsonProperty("name")]
                public string Name { get; set; }

                [JsonProperty("token")]
                public Resource Token { get; set; }

                [JsonProperty("miniatures")]
                public List<Resource> Miniatures { get; set; }

                [JsonIgnore]
                public AsyncTexture2D Icon => this.Miniatures?.Any() ?? false ? 
                                                  GameService.Content.GetRenderServiceTexture(this.Miniatures.First().IconUrl) : 
                                                  this.Token?.Icon ?? GameService.Content.DatAssetCache.GetTextureFromAssetId(1302744);

                public Event() {
                    this.Miniatures = new List<Resource>();
                }

                public List<Resource> GetTokens() {

                    var result = Enumerable.Empty<Resource>().ToList();

                    if (this.Token != null) {
                        result.Add(this.Token);
                    }

                    return result;
                } 
            }

        }
    }
}
