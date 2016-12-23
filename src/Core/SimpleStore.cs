namespace LostTech.NKeyValue
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public sealed class SimpleStore<TKey, TValue>: IKeyValueStore<TKey, TValue>
        where TValue: class
    {
        readonly IKeyValueStore<TKey, IDictionary<string, object>> store;
        readonly string valueField;

        public async Task<TValue> Get(TKey key)
        {
            var valueDic = await this.store.Get(key).ConfigureAwait(false);
            return (TValue)valueDic?[valueField];
        }
    }
}
