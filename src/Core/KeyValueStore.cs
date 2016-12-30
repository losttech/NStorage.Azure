namespace LostTech.NKeyValue
{
    using System;
    using System.Collections.Generic;

    public static class KeyValueStore
    {
        public static IWriteableKeyValueStore<TKey, TValue> WithKey<TKey, TValue>(
            this IWriteableKeyValueStore<string, TValue> store, Func<TKey, string> keySerializer) =>
            new WriteableKeySerializingStore<TKey, TValue>(store, keySerializer);

        public static IWriteableKeyValueStore<TKey, TValue> WithValue<TKey, TValue>(
            this IWriteableKeyValueStore<TKey, IDictionary<string, object>> store,
            Func<IDictionary<string, object>, TValue> deserializer,
            Action<TValue, IDictionary<string, object>> serializer)
            => new WriteableSerializingStore<TKey, TValue>(store, deserializer, serializer);
    }
}
