namespace LostTech.NKeyValue
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public sealed class ConcurrentDictionaryStore<TKey, TValue> : IConcurrentKeyValueStore<TKey, TValue>
    {
        readonly ConcurrentDictionary<TKey, TaggedEntry<object, TValue>> store = new ConcurrentDictionary<TKey, TaggedEntry<object, TValue>>();
        public Task<TValue> Get(TKey key) => 
            this.store.TryGetValue(key, out TaggedEntry<object, TValue> value) 
                ? Task.FromResult(value.Value) 
                : Task.FromException<TValue>(new KeyNotFoundException());

        public Task<TaggedEntry<object, TValue>> GetTagged(TKey key) =>
            this.store.TryGetValue(key, out TaggedEntry<object, TValue> value)
                ? Task.FromResult(value)
                : Task.FromException<TaggedEntry<object, TValue>>(new KeyNotFoundException());

        public Task Put(TKey key, TValue value)
        {
            var entry = new TaggedEntry<object, TValue>
            {
                Tag = new object(),
                Value = value,
            };
            this.store.AddOrUpdate(key, entry, (_,__) => entry);
            return Task.CompletedTask;
        }

        public Task<bool> Put(TKey key, TValue value, object tag)
        {
            var oldEntry = new TaggedEntry<object, TValue> {Tag = tag};
            var entry = new TaggedEntry<object, TValue>
            {
                Tag = new object(),
                Value = value,
            };
            return Task.FromResult(this.store.TryUpdate(key, entry, oldEntry));
        }
    }
}
