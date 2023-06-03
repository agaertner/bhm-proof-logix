using Blish_HUD;
using Blish_HUD.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models {

    // Path: /api/resources?lang={code}

    public class Resources {

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

        [JsonIgnore]
        public IEnumerable<Raid.Wing> Wings => this.Raids.SelectMany(raid => raid.Wings);

        [JsonIgnore]
        public IEnumerable<Resource> Items => this.Raids
                                                  .SelectMany(raid => raid.Wings)
                                                  .SelectMany(wing => wing.Events)
                                                  .SelectMany(ev => ev.GetTokens())
                                                  .Concat(this.Fractals)
                                                  .Concat(this.GeneralTokens)
                                                  .GroupBy(resource => resource.Id)
                                                  .Select(group => group.First());
    }

    public class Resource {

        [JsonProperty("icon")]
        public string IconUrl { get; set; }

        [JsonIgnore]
        public AsyncTexture2D Icon => !string.IsNullOrEmpty(this.IconUrl) 
                                          ? GameService.Content.DatAssetCache.
                                                        GetTextureFromAssetId(int.Parse(Path.GetFileNameWithoutExtension(this.IconUrl))) 
                                          : ContentService.Textures.TransparentPixel;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
    }

    public class Raid {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("wings")]
        public List<Wing> Wings { get; set; }

        public sealed class Wing {


            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("map_id")]
            public int MapId { get; set; }

            [JsonProperty("events")]
            public List<Event> Events { get; set; }

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
                public AsyncTexture2D Icon => this.Miniatures?
                                                 .FirstOrDefault()?.Icon
                                           ?? this.Token?.Icon 
                                           ?? GameService.Content.DatAssetCache.GetTextureFromAssetId(1302744);

                public List<Resource> GetTokens() {

                    var result = Enumerable.Empty<Resource>().ToList();

                    if (this.Token == null && this.Miniatures == null) {
                        return result;
                    }

                    if (this.Token != null) {
                        result.Add(this.Token);
                    }

                    if (this.Miniatures != null) {
                        result.AddRange(this.Miniatures);
                    }

                    return result;
                } 
            }

        }
    }
}
