namespace LostTech.NKeyValue
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents asynchronous readable and writeable key-value store, that supports version-based concurrency.
    /// </summary>
    /// <typeparam name="TKey">Type of keys</typeparam>
    /// <typeparam name="TVersion">Type of version tags</typeparam>
    /// <typeparam name="TValue">Type of values</typeparam>
    public interface IConcurrentVersionedKeyValueStore<in TKey, TVersion, TValue>: IWriteableKeyValueStore<TKey, TValue>
    {
        /// <summary>
        /// Gets the value, matching the provided key, together with associated version.
        /// </summary>
        /// <param name="key">The key of value to get</param>
        Task<VersionedEntry<TVersion, TValue>> TryGetVersioned(TKey key);
        /// <summary>
        /// Tries to set the value for the specified key.
        /// Operation will be cancelled if <paramref name="versionToUpdate"/> does not match currently stored version for the key.
        /// Returns <c>true</c>, if value was updated.
        /// Returns <c>false</c>, if <paramref name="versionToUpdate"/> does not match stored one.
        /// </summary>
        /// <param name="key">The key to set the value for</param>
        /// <param name="value">New value for the key</param>
        /// <param name="versionToUpdate">Stored value must be of this version to be updated.
        /// If stored version is different, update is cancelled, and function returns <c>false</c></param>
        Task<bool> Put(TKey key, TValue value, TVersion versionToUpdate);
    }

    public interface IConcurrentVersionedKeyValueStore<in TKey, TValue> : IConcurrentVersionedKeyValueStore<TKey, object, TValue>
    {
    }
}
