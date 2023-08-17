namespace BankingWebApi.Infrastructure.Extensions
{
    internal static class LinqExtensions
    {
        public static IOrderedEnumerable<TSource> OrderByDynamic<TSource, TKey>
            (this IEnumerable<TSource> source,
             Func<TSource, TKey> keySelector,
             bool ascending)
        {
            return ascending ? source.OrderBy(keySelector)
                             : source.OrderByDescending(keySelector);
        }

        public static IOrderedEnumerable<TSource> ThenByDynamic<TSource, TKey>
            (this IOrderedEnumerable<TSource> source,
             Func<TSource, TKey> keySelector,
             bool ascending)
        {
            return ascending ? source.ThenBy(keySelector)
                             : source.ThenByDescending(keySelector);
        }
    }
}
