namespace LostTech.NKeyValue
{
    using System.Threading.Tasks;

    public interface IKeyValueStore<TKey, TValue>
    {
        Task<TValue> Get(TKey key);
    }
}
