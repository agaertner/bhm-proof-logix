using System.Globalization;
using System.Text.RegularExpressions;
namespace Nekres.ProofLogix.Core {
    public static class StringExtensions {
        public static string ToTitleCase(this string title) {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
        }

        public static string SplitCamelCase(this string input) {
            return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
        }

        public static string ReplaceWhitespace(this string input, string replacement) {
            return Regex.Replace(input, @"\s+", replacement);
        }
    }
}
