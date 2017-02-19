namespace LostTech.NKeyValue.Wrappers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class ConcurrentVersionedSerializingStore<TKey, TVersion, TValue> : WriteableSerializingStore<TKey, TValue>,
        IConcurrentVersionedKeyValueStore<TKey, TVersion, TValue>
    {
        readonly IConcurrentVersionedKeyValueStore<TKey, TVersion, IDictionary<string, object>> backingStore;

        public ConcurrentVersionedSerializingStore(
            IConcurrentVersionedKeyValueStore<TKey, TVersion, IDictionary<string, object>> backingStore,
            Func<IDictionary<string, object>, TValue> deserializer,
            Action<TValue, IDictionary<string, object>> serializer)
            : base(backingStore, deserializer, serializer)
        {
            this.backingStore = backingStore;
        }

        public Task<bool> Put(TKey key, TValue value, TVersion versionToUpdate)
        {
            this.Serialize(value);
            return this.backingStore.Put(key, this.serializedValue, versionToUpdate);
        }

        public async Task<VersionedEntry<TVersion, TValue>> TryGetVersioned(TKey key)
        {
            var entry = await this.backingStore.TryGetVersioned(key).ConfigureAwait(false);
            return entry?.Select(this.deserializer);
        }
    }
}
