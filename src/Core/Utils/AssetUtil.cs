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
    }
}
