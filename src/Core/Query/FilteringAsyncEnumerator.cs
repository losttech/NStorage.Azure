namespace LostTech.NKeyValue.Query
{
    using System;
    using System.Threading.Tasks;

    struct FilteringAsyncEnumerator<T> : IAsyncQueryResultEnumerator<T>
    {
        readonly IAsyncQueryResultEnumerator<T> enumerator;
        readonly Func<T, bool> predicate;

        public FilteringAsyncEnumerator(IAsyncQueryResultEnumerator<T> enumerator, Func<T, bool> predicate)
        {
            this.enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
            this.predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        public T Current => this.enumerator.Current;

        public async Task<bool> MoveNext()
        {
            while (await this.enumerator.MoveNext().ConfigureAwait(false))
            {
                if (this.predicate(this.Current))
                    return true;
            }
            return false;
        }
    }
}
