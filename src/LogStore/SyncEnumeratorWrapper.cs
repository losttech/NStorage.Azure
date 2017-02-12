namespace LostTech.NKeyValue
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class SyncEnumeratorWrapper<T> : IAsyncQueryResultEnumerator<T>
    {
        readonly IEnumerator<T> enumerator;
        public SyncEnumeratorWrapper(IEnumerator<T> enumerator)
        {
            this.enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
        }
        public T Current => this.enumerator.Current;
        public Task<bool> MoveNext() => Task.FromResult(this.enumerator.MoveNext());
    }
}
