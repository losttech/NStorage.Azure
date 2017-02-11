namespace LostTech.NKeyValue.Wrappers
{
    using System;
    using System.Threading.Tasks;

    class WriteableKeySerializingStore<TKey, TValue>: KeySerializingStore<TKey, TValue>,
        IWriteableKeyValueStore<TKey, TValue>
    {
        readonly IWriteableKeyValueStore<string, TValue> store;

        public WriteableKeySerializingStore(IWriteableKeyValueStore<string, TValue> store,
            Func<TKey, string> keySerializer):base(store, keySerializer)
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
