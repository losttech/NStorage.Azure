namespace LostTech.NKeyValue
{
    using System.Threading.Tasks;

    public interface IWriteableKeyValueStore<in TKey, TValue> : IKeyValueStore<TKey, TValue>
    {
        Task Put(TKey key, TValue value);
    }
}
