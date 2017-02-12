namespace LostTech.NKeyValue
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed class SortedLog<T> : ILogStore<T>
    {
        readonly SortedList<SortedLogEntryKey, T> entries = new SortedList<SortedLogEntryKey, T>();

        public Task Add(LogEntry<T> entry)
        {
            this.entries.Add(new SortedLogEntryKey(entry.Timestamp, entry.CorrelationID), entry.Value);
            return Task.CompletedTask;
        }

        public IAsyncQueryResultEnumerator<LogEntry<T>> Query(DateTimeOffset start, DateTimeOffset end, string correlationID)
        {
            var result = entries.SkipWhile(key => key.Key.CompareTo(start) < 0).TakeWhile(key => key.Key.CompareTo(end) <= 0);
            if (correlationID != null)
                result = result.Where(key => key.Key.CorrelationID == correlationID);
            return new SyncEnumeratorWrapper<LogEntry<T>>(result.ToArray().Select(entry => new LogEntry<T>
            {
                Timestamp = entry.Key.Timestamp,
                CorrelationID = entry.Key.CorrelationID,
                Value = entry.Value,
            }).GetEnumerator());
        }

        struct SortedLogEntryKey: IComparable<SortedLogEntryKey>, IComparable<DateTimeOffset>
        {
            public SortedLogEntryKey(DateTimeOffset timestamp, string correlationID) {
                this.Timestamp = timestamp;
                this.CorrelationID = correlationID ?? throw new ArgumentNullException(nameof(correlationID));
            }

            public DateTimeOffset Timestamp { get; }
            public string CorrelationID { get; }

            public int CompareTo(SortedLogEntryKey other)
            {
                int result = this.Timestamp.CompareTo(other.Timestamp);
                if (result != 0)
                    return result;
                return this.CorrelationID.CompareTo(other.CorrelationID);
            }

            public int CompareTo(DateTimeOffset other)
            {
                int result = this.Timestamp.CompareTo(other);
                return result == 0 ? 1 : result;
            }
        }
    }
}
