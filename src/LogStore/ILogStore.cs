namespace LostTech.NKeyValue
{
    using System;
    using System.Threading.Tasks;

    public interface ILogStore<T>
    {
        Task Add(LogEntry<T> entry);
        IAsyncQueryResultEnumerator<LogEntry<T>> Query(DateTimeOffset start, DateTimeOffset end, string correlationID);
    }
}
