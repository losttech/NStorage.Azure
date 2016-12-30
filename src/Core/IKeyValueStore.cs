namespace LostTech.NKeyValue
{
    using System.Threading.Tasks;

    public interface IKeyValueStore<in TKey, TValue>
    {
        Task<TValue> Get(TKey key);
    }
}
