namespace LostTech.NKeyValue.Wrappers
{
    using System;
    using System.Threading.Tasks;

    class KeySerializingStore<TKey, TOldKey, TValue> : IKeyValueStore<TKey, TValue>
    {
        readonly IKeyValueStore<TOldKey, TValue> store;
        protected readonly Func<TKey, TOldKey> keySerializer;

        public KeySerializingStore(IKeyValueStore<TOldKey, TValue> store,
            Func<TKey, TOldKey> keySerializer)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.keySerializer = keySerializer ?? throw new ArgumentNullException(nameof(keySerializer));
        }

        public Task<TValue> Get(TKey key)
        {
            var serializedKey = this.keySerializer(key);
            return this.store.Get(serializedKey);
        }

        public Task<(bool, TValue)> TryGet(TKey key)
        {
            var serializedKey = this.keySerializer(key);
            return this.store.TryGet(serializedKey);
        }
    }
}
