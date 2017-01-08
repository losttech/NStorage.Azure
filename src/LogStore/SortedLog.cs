namespace LostTech.NKeyValue
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public sealed class SortedLog<T> : ILogStore<T>
    {
        readonly SortedList<DateTimeOffset, T> entries = new SortedList<DateTimeOffset, T>();

        public Task Add(DateTimeOffset timestamp, T entry)
        {
            this.entries.Add(timestamp, entry);
            return Task.CompletedTask;
        }
    }
}
