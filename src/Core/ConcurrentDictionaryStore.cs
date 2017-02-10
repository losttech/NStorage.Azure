namespace LostTech.NKeyValue
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Implements in-memory store, backed by <see cref="ConcurrentDictionary{TKey,TValue}"/>
    /// </summary>
    /// <typeparam name="TKey">Type of the key</typeparam>
    /// <typeparam name="TValue">Type of the value</typeparam>
    /// <remarks>Instances of this class are thread-safe</remarks>
    public sealed class ConcurrentDictionaryStore<TKey, TValue> : IConcurrentVersionedKeyValueStore<TKey, TValue>
    {
        readonly ConcurrentDictionary<TKey, VersionedEntry<object, TValue>> store = new ConcurrentDictionary<TKey, VersionedEntry<object, TValue>>();
        public Task<TValue> Get(TKey key) => 
            this.store.TryGetValue(key, out VersionedEntry<object, TValue> value) 
                ? Task.FromResult(value.Value) 
                : Task.FromException<TValue>(new KeyNotFoundException());

        public Task<(bool, TValue)> TryGet(TKey key)
        {
            bool found = this.store.TryGetValue(key, out VersionedEntry<object, TValue> value);
            return Task.FromResult((found, (found ? value.Value : default(TValue))));
        }

        public Task<VersionedEntry<object, TValue>> TryGetVersioned(TKey key) =>
            Task.FromResult(this.store.TryGetValue(key, out VersionedEntry<object, TValue> value) ? value : null);

        public Task Put(TKey key, TValue value)
        {
            var entry = new VersionedEntry<object, TValue>
            {
                Version = new object(),
                Value = value,
            };
            this.store.AddOrUpdate(key, entry, (_,__) => entry);
            return Task.CompletedTask;
        }

        public Task<bool> Put(TKey key, TValue value, object version)
        {
            var oldEntry = new VersionedEntry<object, TValue> {Version = version};
            var entry = new VersionedEntry<object, TValue>
            {
                Version = new object(),
                Value = value,
            };
            return Task.FromResult(
                version == null
                    ? this.store.TryAdd(key, entry)
                    : this.store.TryUpdate(key, entry, oldEntry));
        }
    }
}
