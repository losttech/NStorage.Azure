namespace LostTech.NKeyValue
{
    using System;

    public struct PartitionedKey<TPartition, TRow>
        : IEquatable<PartitionedKey<TPartition, TRow>>
    {
        public PartitionedKey(TPartition partition, TRow row)
        {
            this.Partition = partition;
            this.Row = row;
        }
        public TPartition Partition { get; }
        public TRow Row { get; }

        public bool Equals(PartitionedKey<TPartition, TRow> other) =>
            this.Partition.Equals(other.Partition)
            && this.Row.Equals(other.Row);

        public override bool Equals(object obj)
        {
            if (obj is PartitionedKey<TPartition, TRow> other)
                return this.Equals(other);
            return false;
        }

        public override int GetHashCode() => this.Partition.GetHashCode() * 31 + this.Row.GetHashCode();
    }
}
