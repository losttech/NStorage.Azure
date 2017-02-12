namespace LostTech.NKeyValue
{
    using System;
    using System.Threading.Tasks;

    public interface IAsyncQueryResultEnumerator<T>
    {
        Task<bool> MoveNext();
        T Current { get; }
    }
}
