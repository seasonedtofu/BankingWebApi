namespace BankingWebApi.Extensions
{
    internal static class LinqExtensions
    {
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>
            (this IEnumerable<TSource> source,
             Func<TSource, TKey> keySelector,
             bool ascending)
        {
            return ascending ? source.OrderBy(keySelector)
                             : source.OrderByDescending(keySelector);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>
            (this IOrderedEnumerable<TSource> source,
             Func<TSource, TKey> keySelector,
             bool ascending)
        {
            return ascending ? source.ThenBy(keySelector)
                             : source.ThenByDescending(keySelector);
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>
            (this IQueryable<TSource> source,
             Func<TSource, TKey> keySelector,
             bool ascending)
        {
            return ascending ? source.OrderBy(keySelector)
                             : source.OrderByDescending(keySelector);
        }
    }
}
