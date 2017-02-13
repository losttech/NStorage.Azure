namespace LostTech.NKeyValue.Query
{
    using System;
    using System.Threading.Tasks;

    struct MappingAsyncEnumerator<TSource, TFinal> : IAsyncQueryResultEnumerator<TFinal>
    {
        readonly IAsyncQueryResultEnumerator<TSource> enumerator;
        readonly Func<TSource, TFinal> map;

        public MappingAsyncEnumerator(IAsyncQueryResultEnumerator<TSource> enumerator, Func<TSource, TFinal> map)
        {
            this.enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
            this.map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public TFinal Current => this.map(this.enumerator.Current);

        public Task<bool> MoveNext() => this.enumerator.MoveNext();
    }
}
