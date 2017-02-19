namespace LostTech.NKeyValue
{
    using System.Collections.Generic;

    public interface IPartitionedKeyValueStore<TPartition, TRow, TValue>
    {
        IAsyncQueryResultEnumerator<KeyValuePair<PartitionedKey<TPartition, TRow>, TValue>>
            Query(Range<TPartition> partitionRange, Range<TRow> rowRange);
    }
}
