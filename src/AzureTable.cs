namespace LostTech.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Key = PartitionedKey<string, string>;
    using Value = System.Collections.Generic.IDictionary<string, object>;
    using Partition = System.String;
    using Row = System.String;
    using Version = System.String;

    public sealed class AzureTable:
        IConcurrentVersionedKeyValueStore<Key, Version, Value>,
        IPartitionConcurrentVersionedKeyValueStore<Partition, Row, Version, Value>,
        IPartitionedKeyValueStore<Partition, Row, Value>
    {
        const string EntityCanOnlyAppearOnceInBatch = "An entity can only appear once in batch operation";
        const string TooManyEntitiesMessage = "Too many entities. Supported maximum is 100";
        readonly CloudTable table;

        public AzureTable(CloudTable table)
        {
            this.table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public async Task<Value> Get(Key key)
        {
            CheckKey(key);
            var (found, result) = await this.TryGet(key).ConfigureAwait(false);
            return found ? result : throw new KeyNotFoundException();
        }

        public async Task<(bool, Value)> TryGet(Key key)
        {
            CheckKey(key);

            var query = MakeQueryByKey(key);
            var resultSet = await this.table.ExecuteQuerySegmentedAsync(query, null).ConfigureAwait(false);
            var result = resultSet.Results.FirstOrDefault();
            if (result == null)
                return (false, null);
            return (true, ParseValue(result));
        }

        static Dictionary<string, object> ParseValue(DynamicTableEntity result) =>
            result.Properties
                .Where(kv => AzureTableEntity.IsKey(kv.Key))
                .ToDictionary(kv => AzureTableEntity.DecodeKey(kv.Key), kv => kv.Value.PropertyAsObject);

        private static TableQuery MakeQueryByKey(Key key)
        {
            key = KeyEncode(key);
            return new TableQuery().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.Equal, key.Row),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, key.Partition)));
        }

        public Task Put(Key key, Value value)
        {
            CheckKey(key);
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            key = KeyEncode(key);
            var entity = new AzureTableEntity(key, value);
            return this.table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }

        public static async Task<AzureTable> OpenOrCreate(string accountConnectionString, string tableName)
        {
            if (string.IsNullOrEmpty(accountConnectionString))
                throw new ArgumentNullException(nameof(accountConnectionString));
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException(nameof(tableName));

            var account = CloudStorageAccount.Parse(accountConnectionString);
            var tableClient = account.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);
            return new AzureTable(table);
        }

        public async Task<VersionedEntry<string, Value>> TryGetVersioned(Key key)
        {
            CheckKey(key);

            var query = MakeQueryByKey(key);
            var resultSet = await this.table.ExecuteQuerySegmentedAsync(query, null).ConfigureAwait(false);
            var result = resultSet.Results.FirstOrDefault();
            if (result == null)
                return null;

            return new VersionedEntry<string, Value>
            {
                Version = result.ETag,
                Value = ParseValue(result),
            };
        }

        public async Task<(bool, string)> Put(Key key, Value value, string versionToUpdate)
        {
            CheckKey(key);
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var operation = MakePutOperation(key, value, versionToUpdate);
            try
            {
                var result = await this.table.ExecuteAsync(operation).ConfigureAwait(false);
                // .NET is bizzare. In my practice, when there's concurrency violation,
                // StorageException is thrown. However, for some reason TableResult includes status code
                // Wonder, why throw if they could just return the status code? It's a common situation...
                return (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300, result.Etag);
            }
            catch (StorageException exception) 
                when (exception.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed
                    || exception.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                return (false, null);
            }
        }

        static TableOperation MakePutOperation(Key key, Value value, string versionToUpdate)
        {
            key = KeyEncode(key);
            var entity = new AzureTableEntity(key, value);
            TableOperation operation;
            if (versionToUpdate == null) {
                operation = TableOperation.Insert(entity);
            }
            else {
                entity.ETag = versionToUpdate;
                operation = TableOperation.Replace(entity);
            }
            return operation;
        }

        public async Task<bool> Put(Partition partition, KeyValuePair<Row, VersionedEntry<Version, Value>>[] entities)
        {
            CheckPartition(partition);
            CheckEntitySet(entities);
            if (entities.Any(value => value.Value == null))
                throw new ArgumentException(paramName: nameof(entities), message: "Entities must not be null");

            var batch = new TableBatchOperation();
            foreach (var value in entities) {
                var operation = MakePutOperation(new Key(partition, value.Key), value.Value.Value, value.Value.Version);
                batch.Add(operation);
            }

            return await this.ExecuteAsync(batch).ConfigureAwait(false);
        }

        async Task<bool> ExecuteAsync(TableBatchOperation batch)
        {
            try {
                var results = await this.table.ExecuteBatchAsync(batch).ConfigureAwait(false);
                // .NET is bizzare. In my practice, when there's concurrency violation,
                // StorageException is thrown. However, for some reason TableResult includes status code
                // Wonder, why throw if they could just return the status code? It's a common situation...
                return results.All(result => result.HttpStatusCode >= 200 && result.HttpStatusCode < 300);
            }
            catch (StorageException exception)
                when (exception.RequestInformation.HttpStatusCode == (int) HttpStatusCode.PreconditionFailed
                      || exception.RequestInformation.HttpStatusCode == (int) HttpStatusCode.Conflict) {
                return false;
            }
        }

        public async Task<bool> Delete(Key key, string versionToDelete)
        {
            CheckKey(key);

            var operation = MakeDeleteOperation(key, versionToDelete);
            try {
                var result = await this.table.ExecuteAsync(operation).ConfigureAwait(false);
                // .NET is bizzare. In my practice, when there's concurrency violation,
                // StorageException is thrown. However, for some reason TableResult includes status code
                // Wonder, why throw if they could just return the status code? It's a common situation...
                return result.HttpStatusCode >= 200 && result.HttpStatusCode < 300;
            } catch (StorageException exception)
                  when (exception.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed
                      || exception.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict) {
                return false;
            }
        }

        public async Task<bool> Delete(Partition partition, KeyValuePair<Row, Version>[] entities)
        {
            CheckPartition(partition);
            CheckEntitySet(entities);
            if (entities.Length == 0)
                return true;

            var batch = new TableBatchOperation();
            foreach (var entity in entities) {
                batch.Add(MakeDeleteOperation(new Key(partition, entity.Key), entity.Value));
            }
            return await ExecuteAsync(batch).ConfigureAwait(false);
        }

        static TableOperation MakeDeleteOperation(Key key, string versionToDelete)
        {
            key = KeyEncode(key);
            var entity = AzureTableEntity.KeyOnly(key);
            entity.ETag = versionToDelete;
            return TableOperation.Delete(entity);
        }

        public async Task<bool?> Delete(Key key)
        {
            CheckKey(key);

            key = KeyEncode(key);
            var entity = AzureTableEntity.KeyOnly(key);
            await this.table.ExecuteAsync(TableOperation.Delete(entity)).ConfigureAwait(false);
            // TODO: implement return code
            return null;
        }

        internal static void CheckKey(Key key)
        {
            if (string.IsNullOrEmpty(key.Partition))
                throw new ArgumentNullException(nameof(key.Partition));
            if (string.IsNullOrEmpty(key.Row))
                throw new ArgumentNullException(nameof(key.Row));
        }

        internal static void CheckPartition(Partition partition)
        {
            if (string.IsNullOrEmpty(partition))
                throw new ArgumentNullException(nameof(partition));
        }

        static void CheckEntitySet<TValue>(KeyValuePair<Row, TValue>[] entities) {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            if (entities.Any(value => value.Key == null))
                throw new ArgumentException(paramName: nameof(entities), message: "Entity row keys must not be null");
            if (entities.Length > 100)
                throw new ArgumentOutOfRangeException(paramName: nameof(entities),
                    message: TooManyEntitiesMessage);
            if (entities.Select(value => value.Key).Distinct().Count() < entities.Length)
                throw new NotSupportedException(EntityCanOnlyAppearOnceInBatch);
        }

        public int? PageSizeLimit => 1000;

        public async Task<PagedQueryResult<KeyValuePair<Key, Value>>> Query(Range<string> partitionRange, Range<string> rowRange,
            int? pageSize = null, object continuationToken = null)
        {
            if (continuationToken != null && !(continuationToken is TableContinuationToken))
                throw new ArgumentException(paramName: nameof(continuationToken), message: "Unexpected type");
            if ((pageSize ?? 1) <= 0)
                throw new ArgumentOutOfRangeException(paramName: nameof(pageSize));
            if ((PageSizeLimit ?? int.MaxValue) < (pageSize ?? 1))
                throw new ArgumentOutOfRangeException(paramName: nameof(pageSize), message: "Too many records requested");

            var filter = TableQuery.CombineFilters(
                RangeFilter(nameof(ITableEntity.RowKey), rowRange),
                TableOperators.And,
                RangeFilter(nameof(ITableEntity.PartitionKey), partitionRange));

            var query = new TableQuery {
                TakeCount = pageSize,
            }.Where(filter);

            var page = await this.table.ExecuteQuerySegmentedAsync(query, (TableContinuationToken)continuationToken).ConfigureAwait(false);
            var results = page.Results.Select(entity => new KeyValuePair<Key, Value>(
                    key: new Key(partition: KeyDecode(entity.PartitionKey), row: KeyDecode(entity.RowKey)),
                    value: ParseValue(entity)
                )).ToArray();
            return new PagedQueryResult<KeyValuePair<Key, Value>>(results, page.ContinuationToken);
        }

        static string RangeFilter(string propertyName, Range<string> range) =>
            TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(propertyName, QueryComparisons.GreaterThanOrEqual, KeyEncode(range.Start)),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(propertyName, QueryComparisons.GreaterThanOrEqual, KeyEncode(range.End)));

        static readonly Regex UnsupportedKeyCharRegex = new Regex(@"[\\/#\?%]", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        static readonly Regex UnsupportedEscapedKeyCharRegex = new Regex(@"\%[a-f0-9]{2}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string KeyEncode(string key) =>
            string.IsNullOrEmpty(key)
            ? throw new ArgumentNullException(nameof(key))
            : UnsupportedKeyCharRegex.Replace(key, badMatch => $"%{(byte)badMatch.Value[0]:x2}");
        static Key KeyEncode(Key key) => new Key(partition: KeyEncode(key.Partition), row: KeyEncode(key.Row));
        public static string KeyDecode(string key) =>
            string.IsNullOrEmpty(key)
            ? throw new ArgumentNullException(nameof(key))
            : UnsupportedEscapedKeyCharRegex.Replace(key, badMatch => ((char)Byte.Parse(badMatch.Value.Substring(1), NumberStyles.HexNumber)).ToString());
        static Key KeyDecode(Key key) => new Key(partition: KeyDecode(key.Partition), row: KeyDecode(key.Row));
    }
}