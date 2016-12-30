namespace LostTech.NKeyValue
{
    using System;
    using System.Threading.Tasks;

    class KeySerializingStore<TKey, TValue> : IKeyValueStore<TKey, TValue>
    {
        readonly IKeyValueStore<string, TValue> store;
        protected readonly Func<TKey, string> keySerializer;

        public KeySerializingStore(IKeyValueStore<string, TValue> store,
            Func<TKey, string> keySerializer)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.keySerializer = keySerializer ?? throw new ArgumentNullException(nameof(keySerializer));
        }

        public Task<TValue> Get(TKey key)
        {
            var serializedKey = this.keySerializer(key);
            return this.store.Get(serializedKey);
        }
    }
}
