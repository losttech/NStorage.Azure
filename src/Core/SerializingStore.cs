namespace LostTech.NKeyValue
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class SerializingStore<TKey, TValue> : IKeyValueStore<TKey, TValue>
    {
        readonly IKeyValueStore<TKey, IDictionary<string, object>> store;
        readonly Func<IDictionary<string, object>, TValue> deserializer;

        public SerializingStore(IKeyValueStore<TKey, IDictionary<string, object>> backingStore,
            Func<IDictionary<string, object>, TValue> deserializer)
        {
            this.store = backingStore ?? throw new ArgumentNullException(nameof(backingStore));
            this.deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        }

        public async Task<TValue> Get(TKey key)
        {
            var serialized = await this.store.Get(key).ConfigureAwait(false);
            return deserializer(serialized);
        }
    }
}
