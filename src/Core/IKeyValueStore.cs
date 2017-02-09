namespace LostTech.NKeyValue
{
    using System.Threading.Tasks;

    public interface IKeyValueStore<in TKey, TValue>
    {
        Task<TValue> Get(TKey key);
        Task<(bool, TValue)> TryGet(TKey key);
    }
}
