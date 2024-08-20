using System.Text.RegularExpressions;
namespace Nekres.ProofLogix.Core {
    public static class StringExtensions {
        public static string SplitCamelCase(this string input) {
            return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
        }
    }
}
