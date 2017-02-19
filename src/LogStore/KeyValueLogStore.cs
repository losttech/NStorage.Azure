namespace LostTech.NKeyValue
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using Key = PartitionedKey<string, string>;

    public sealed class KeyValueLogStore<T, TStore> : ILogStore<T>
        where TStore: class,
            IWriteableKeyValueStore<PartitionedKey<string, string>, T>,
            IPartitionedKeyValueStore<string, string, T>
    {
        readonly TStore store;
        readonly long timeGroupingInterval = TimeSpan.FromMinutes(5).Ticks;
        int suffix = 0;

        public KeyValueLogStore(TStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public Task Add(LogEntry<T> entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));
            if (entry.CorrelationID == null)
                throw new ArgumentNullException(nameof(entry.CorrelationID));

            var stamp = entry.Timestamp.UtcTicks;
            string correlationId = entry.CorrelationID;
            string partition = MakePartitionKey(stamp, correlationId);
            int suffix = Interlocked.Increment(ref this.suffix);
            string key = Hex64(stamp) + Hex32(suffix);

            return this.store.Put(new Key(partition: partition, row: key), entry.Value);
        }

        string MakePartitionKey(long stamp, string correlationId)
        {
            var timeGroup = RoundDown(stamp, this.timeGroupingInterval);
            return Hex64(timeGroup) + correlationId;
        }

        static string GetCorrelationId(string partitionKey) => partitionKey.Substring(16);
        static DateTimeOffset GetTimestamp(string key) => new DateTimeOffset(Convert.ToInt64(key.Substring(0, 16), 16), TimeSpan.Zero);

        public IAsyncQueryResultEnumerator<LogEntry<T>> Query(DateTimeOffset start, DateTimeOffset end, string correlationID)
        {
            if (start > end)
                throw new ArgumentException(message: $"{nameof(end)} must not be less than {nameof(start)}");

            var keyRange = new Range<string>(
                start: Hex64(start.UtcTicks) + Hex32(0),
                end: Hex64(end.UtcTicks) + Hex32(-1));
            long startTimeGroup = RoundDown(start.UtcTicks, this.timeGroupingInterval);
            long endTimeGroup = RoundDown(end.UtcTicks+1, this.timeGroupingInterval);
            var paritionRange = new Range<string>(
                start: Hex64(startTimeGroup) + (correlationID ?? ""),
                end: correlationID == null ? Hex64(endTimeGroup + 1): Hex64(endTimeGroup) + correlationID);
            return this.store.Query(paritionRange, keyRange)
                .Select(ParseQueryResult)
                .Where(entry => entry.Timestamp >= start && entry.Timestamp <= end 
                            && (correlationID == null || entry.CorrelationID == correlationID));
        }

        static LogEntry<T> ParseQueryResult(KeyValuePair<Key, T> entry)
        {
            return new LogEntry<T>
            {
                CorrelationID = GetCorrelationId(partitionKey: entry.Key.Partition),
                Timestamp = GetTimestamp(key: entry.Key.Row),
                Value = entry.Value,
            };
        }

        static string Hex64(long value) => value.ToString("X16", CultureInfo.InvariantCulture);
        static string Hex32(int value) => value.ToString("X8", CultureInfo.InvariantCulture);

        static long RoundDown(long value, long rounding)
        {
            if (rounding <= 0)
                throw new ArgumentOutOfRangeException(nameof(rounding));
            return (value / rounding) * rounding;
        }
    }
}
