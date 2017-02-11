namespace LostTech.NKeyValue.InMemory
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements in-memory store, backed by <see cref="Dictionary{TKey,TValue}"/>
    /// </summary>
    /// <typeparam name="TKey">Type of the key</typeparam>
    /// <typeparam name="TValue">Type of the value</typeparam>
    /// <remarks>Instances of this class are not thread-safe</remarks>
    public sealed class DictionaryStore<TKey, TValue> : IWriteableKeyValueStore<TKey, TValue>
    {
        readonly Dictionary<TKey, TValue> store = new Dictionary<TKey,TValue>();

        public Task<TValue> Get(TKey key)
        {
            if (this.store.TryGetValue(key, out var value))
                return Task.FromResult(value);

            return Task.FromException<TValue>(new KeyNotFoundException());
        }

        public Task<(bool, TValue)> TryGet(TKey key)
        {
            bool found = this.store.TryGetValue(key, out TValue value);
            return Task.FromResult((found, value));
        }

        public Task Put(TKey key, TValue value)
        {
            this.store[key] = value;
            return Task.CompletedTask;
        }
    }
}
