using Blish_HUD;
using Blish_HUD.Extended;
using Gw2Sharp.WebApi;
using MonoGame.Extended.BitmapFonts;
using System.IO;

namespace Nekres.ProofLogix.Core {
    public static class AssetUtil {

        private const char ELLIPSIS      = '\u2026';
        private const char BRACKET_LEFT  = '[';
        private const char BRACKET_RIGHT = ']';

        public static int GetId(string assetUri) {
            return int.Parse(Path.GetFileNameWithoutExtension(assetUri));
        }

        public static string GetItemDisplayName(string name, int quantity, bool brackets = true) {
            if (quantity == 1) {
                return brackets ? $"{BRACKET_LEFT}{name}{BRACKET_RIGHT}" : name;
            }
            return brackets ? $"{BRACKET_LEFT}{quantity} {name}{BRACKET_RIGHT}" : $"{quantity} {name}";
        }

        public static string Truncate(string text, int maxWidth, BitmapFont font) {
            var result = text;
            var width  = (int)font.MeasureString(result).Width;
            while (width > maxWidth) {
                result = result.Substring(0, result.Length - 1);
                width  = (int)font.MeasureString(result).Width;
            }
            return result.Length < text.Length ? result.TrimEnd() + ELLIPSIS : result;
        }

        public static string GetWikiLink(string wikiPage) {
            switch (GameService.Overlay.UserLocale.Value) {
                case Locale.English:
                case Locale.Spanish:
                case Locale.German:
                case Locale.French: 
                    return $"https://wiki-{GameService.Overlay.UserLocale.Value.Code()}.guildwars2.com/wiki/{wikiPage}";
                case Locale.Korean:
                case Locale.Chinese:
                default: return $"https://wiki.guildwars2.com/index.php?search={wikiPage}";
            }
        }
    }
}
