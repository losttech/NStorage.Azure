namespace LostTech.NKeyValue
{
    using System;
    using System.Threading.Tasks;

    public interface IConcurrentKeyValueStore<in TKey, TTag, TValue>: IWriteableKeyValueStore<TKey, TValue>
    {
        Task<TaggedEntry<TTag, TValue>> TryGetTagged(TKey key);
        Task<bool> Put(TKey key, TValue value, TTag tag);
    }

    public interface IConcurrentKeyValueStore<in TKey, TValue> : IWriteableKeyValueStore<TKey, TValue>
    {
        Task<TaggedEntry<object, TValue>> TryGetTagged(TKey key);
        Task<bool> Put(TKey key, TValue value, object tag);
    }
}
