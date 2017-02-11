namespace LostTech.NKeyValue.Wrappers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class WriteableSerializingStore<TKey, TValue> : SerializingStore<TKey, TValue>,
        IWriteableKeyValueStore<TKey, TValue>
    {
        readonly IWriteableKeyValueStore<TKey, IDictionary<string, object>> backingStore;
        readonly Action<TValue, IDictionary<string, object>> serializer;
        protected readonly IDictionary<string, object> serializedValue = new Dictionary<string, object>();

        public WriteableSerializingStore(IWriteableKeyValueStore<TKey, IDictionary<string, object>> backingStore, 
            Func<IDictionary<string, object>, TValue> deserializer,
            Action<TValue, IDictionary<string, object>> serializer)
            : base(backingStore, deserializer)
        {
            this.backingStore = backingStore;
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public Task Put(TKey key, TValue value)
        {
            this.serializedValue.Clear();
            this.serializer(value, this.serializedValue);
            return this.backingStore.Put(key, this.serializedValue);
        }

        protected void Serialize(TValue value)
        {
            this.serializedValue.Clear();
            this.serializer(value, this.serializedValue);
        }
    }
}
