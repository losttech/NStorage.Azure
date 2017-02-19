namespace LostTech.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Table;
    using Key = PartitionedKey<string, string>;
    using Value = System.Collections.Generic.IDictionary<string, object>;

    class AzureTableSegmentedEnumerator : IAsyncQueryResultEnumerator<KeyValuePair<Key, Value>>
    {
        IEnumerator<DynamicTableEntity> currentEnumerator;
        TableQuerySegment currentSegment;
        Task<TableQuerySegment> nextSegment;
        readonly Func<TableContinuationToken, Task<TableQuerySegment>> pager;

        public AzureTableSegmentedEnumerator(Func<TableContinuationToken, Task<TableQuerySegment>> pager,
            Task<TableQuerySegment> firstSegment)
        {
            this.pager = pager ?? throw new ArgumentNullException(nameof(pager));
            this.nextSegment = firstSegment ?? throw new ArgumentNullException(nameof(firstSegment));
        }

        public KeyValuePair<Key, Value> Current => this.currentEnumerator == null
            ? throw new InvalidOperationException()
            : ToKeyValue(this.currentEnumerator.Current);

        public async Task<bool> MoveNext()
        {
            while (true) {
                if (this.currentEnumerator == null) {
                    this.currentSegment = await this.nextSegment.ConfigureAwait(false);
                    this.currentEnumerator = this.currentSegment.GetEnumerator();
                }

                if (this.currentEnumerator.MoveNext())
                    return true;

                this.currentEnumerator = null;

                if (this.currentSegment.ContinuationToken == null)
                    return false;

                this.currentSegment = null;

                this.nextSegment = this.pager(this.currentSegment.ContinuationToken);
            }
        }

        private static KeyValuePair<Key, Value> ToKeyValue(DynamicTableEntity entity)
        {
            var key = new Key(row: entity.RowKey, partition: entity.PartitionKey);
            var value = entity.Properties.ToDictionary(kv => kv.Key, kv => kv.Value.PropertyAsObject);
            return new KeyValuePair<Key, Value>(key, value);
        }
    }
}
