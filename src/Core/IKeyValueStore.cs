namespace LostTech.NKeyValue
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents asynchronous readable key-value store
    /// </summary>
    /// <typeparam name="TKey">Type of keys</typeparam>
    /// <typeparam name="TValue">Type of values</typeparam>
    public interface IKeyValueStore<in TKey, TValue>
    {
        /// <summary>
        /// Gets value matching the provided key.
        /// If key is not in the store, throws <see cref="KeyNotFoundException"/>
        /// </summary>
        /// <param name="key">The key of the value to get</param>
        Task<TValue> Get(TKey key);
        /// <summary>
        /// Gets the value for the key. Returns a pair of <c>true</c> and the value.
        /// If key is not in the store, the first value in the pair will be <c>false</c>.
        /// </summary>
        /// <param name="key">The key of the value to get</param>
        Task<(bool, TValue)> TryGet(TKey key);
    }
}
