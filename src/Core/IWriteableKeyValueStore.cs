namespace LostTech.NKeyValue
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents asynchronous readable and writeable key-value store
    /// </summary>
    /// <typeparam name="TKey">Type of keys</typeparam>
    /// <typeparam name="TValue">Type of values</typeparam>
    public interface IWriteableKeyValueStore<in TKey, TValue> : IKeyValueStore<TKey, TValue>
    {
        /// <summary>
        /// Sets the value for the specified key.
        /// </summary>
        /// <param name="key">The key to set value for</param>
        /// <param name="value">New value for the key</param>
        Task Put(TKey key, TValue value);
    }
}
