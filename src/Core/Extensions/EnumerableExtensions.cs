using System;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.ProofLogix.Core {
    public static class EnumerableExtensions {
        public static double Median<TColl, TValue>(
            this IEnumerable<TColl> source,
            Func<TColl, TValue>     selector) {
            return source.Select(selector).Median();
        }

        public static double Median<T>(
            this IEnumerable<T> source) {
            if (Nullable.GetUnderlyingType(typeof(T)) != null) {
                source = source.Where(x => x != null);
            }

            int count = source.Count();
            if (count == 0) {
                throw new InvalidOperationException("Sequence contains no elements.");
            }

            source = source.OrderBy(n => n);

            int midpoint = count / 2;
            if (count % 2 == 0) {
                return (Convert.ToDouble(source.ElementAt(midpoint - 1)) + Convert.ToDouble(source.ElementAt(midpoint))) / 2.0;
            }
            return Convert.ToDouble(source.ElementAt(midpoint));
        }
    }
}
