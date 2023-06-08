using System.IO;

namespace Nekres.ProofLogix.Core {
    public static class AssetUtil {
        public static int GetId(string assetUri) {
            return int.Parse(Path.GetFileNameWithoutExtension(assetUri));
        }
    }
}
