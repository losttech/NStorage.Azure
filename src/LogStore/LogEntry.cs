namespace LostTech.NKeyValue
{
    using System;

    public sealed class LogEntry<T>
    {
        public DateTimeOffset Timestamp { get; set; }
        public string CorrelationID { get; set; }
        public T Value { get; set; }
    }
}
