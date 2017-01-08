namespace LostTech.NKeyValue
{
    using System;
    using System.Threading.Tasks;

    public interface ILogStore<T>
    {
        Task Add(DateTimeOffset timestamp, T entry);
    }
}
