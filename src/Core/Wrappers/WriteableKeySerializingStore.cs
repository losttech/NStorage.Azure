namespace LostTech.NKeyValue.Wrappers
{
    using System;
    using System.Threading.Tasks;

    class WriteableKeySerializingStore<TKey, TOldKey, TValue>: KeySerializingStore<TKey, TOldKey, TValue>,
        IWriteableKeyValueStore<TKey, TValue>
    {
        readonly IWriteableKeyValueStore<TOldKey, TValue> store;

        public WriteableKeySerializingStore(IWriteableKeyValueStore<TOldKey, TValue> store,
            Func<TKey, TOldKey> keySerializer):base(store, keySerializer)
        {
            this.store = store;
        }

        public Task Put(TKey key, TValue value)
        {
            var serializedKey = this.keySerializer(key);
            return this.store.Put(serializedKey, value);
        }
    }
}
