using System.Globalization;
namespace Nekres.ProofLogix.Core {
    public static class StringExtensions {
        public static string ToTitleCase(this string title) {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
        }
    }
}
