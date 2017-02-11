namespace LostTech.NKeyValue
{
    using System;
    using System.Collections.Generic;
    using LostTech.NKeyValue.Wrappers;

    public static class KeyValueStore
    {
        /// <summary>
        /// Wraps <see cref="String"/>-keyed store to use <typeparamref name="TKey"/> for keys, using the provided key serializer.
        /// </summary>
        /// <typeparam name="TKey">Type of keys to use</typeparam>
        /// <typeparam name="TValue">Type of store values</typeparam>
        /// <param name="store"><see cref="String"/>-keyed store to wrap</param>
        /// <param name="keySerializer">Serializer, that converts <typeparamref name="TKey"/> keys into string keys</param>
        public static IWriteableKeyValueStore<TKey, TValue> WithKey<TKey, TValue>(
            this IWriteableKeyValueStore<string, TValue> store, Func<TKey, string> keySerializer) =>
            new WriteableKeySerializingStore<TKey, TValue>(store, keySerializer);

        /// <summary>
        /// Wraps dictionary-valued store to use <typeparamref name="TValue"/> for values, using the provided codec.
        /// </summary>
        /// <typeparam name="TKey">Type of store keys</typeparam>
        /// <typeparam name="TValue">Type of values to use</typeparam>
        /// <param name="store">Dictionary-valued store to wrap</param>
        /// <param name="deserializer">Deserializer (decoder) for values</param>
        /// <param name="serializer">Serializer (encoder) for values</param>
        /// <returns></returns>
        public static IWriteableKeyValueStore<TKey, TValue> WithValue<TKey, TValue>(
            this IWriteableKeyValueStore<TKey, IDictionary<string, object>> store,
            Func<IDictionary<string, object>, TValue> deserializer,
            Action<TValue, IDictionary<string, object>> serializer)
            => new WriteableSerializingStore<TKey, TValue>(store, deserializer, serializer);

        /// <summary>
        /// Wraps <see cref="String"/>-keyed store to use <typeparamref name="TKey"/> for keys, using the provided key serializer.
        /// </summary>
        /// <typeparam name="TKey">Type of keys to use</typeparam>
        /// <typeparam name="TValue">Type of store values</typeparam>
        /// <param name="store"><see cref="String"/>-keyed store to wrap</param>
        /// <param name="keySerializer">Serializer, that converts <typeparamref name="TKey"/> keys into string keys</param>
        public static IConcurrentVersionedKeyValueStore<TKey, TVersion, TValue> WithKey<TKey, TVersion, TValue>(
            this IConcurrentVersionedKeyValueStore<string, TVersion, TValue> store, Func<TKey, string> keySerializer) =>
            new ConcurrentVersionedKeySerializingStore<TKey, TVersion, TValue>(store, keySerializer);

        /// <summary>
        /// Wraps dictionary-valued store to use <typeparamref name="TValue"/> for values, using the provided codec.
        /// </summary>
        /// <typeparam name="TKey">Type of store keys</typeparam>
        /// <typeparam name="TValue">Type of values to use</typeparam>
        /// <param name="store">Dictionary-valued store to wrap</param>
        /// <param name="deserializer">Deserializer (decoder) for values</param>
        /// <param name="serializer">Serializer (encoder) for values</param>
        /// <returns></returns>
        public static IConcurrentVersionedKeyValueStore<TKey, TVersion, TValue> WithValue<TKey, TVersion, TValue>(
            this IConcurrentVersionedKeyValueStore<TKey, TVersion, IDictionary<string, object>> store,
            Func<IDictionary<string, object>, TValue> deserializer,
            Action<TValue, IDictionary<string, object>> serializer)
            => new ConcurrentVersionedSerializingStore<TKey, TVersion, TValue>(store, deserializer, serializer);
    }
}
