namespace LostTech.NKeyValue
{
    using System;
    using System.Collections.Generic;
    using LostTech.NKeyValue.Query;

    public static class AsyncEnumeratorWrappers
    {
        public static IAsyncQueryResultEnumerator<T> Where<T>(this IAsyncQueryResultEnumerator<T> enumerator,
            Func<T, bool> predicate)
            => new FilteringAsyncEnumerator<T>(enumerator, predicate);

        public static IAsyncQueryResultEnumerator<TFinal> Select<TSource, TFinal>(this IAsyncQueryResultEnumerator<TSource> enumerator,
            Func<TSource, TFinal> mapper)
            => new MappingAsyncEnumerator<TSource,TFinal>(enumerator, mapper);
    }
}
