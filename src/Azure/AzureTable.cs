namespace LostTech.NKeyValue
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Key = PartitionedKey<string, string>;

    public sealed class AzureTable: IConcurrentVersionedKeyValueStore<Key, string, IDictionary<string, object>>
    {
        readonly CloudTable table;

        private AzureTable(CloudTable table)
        {
            this.table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public async Task<IDictionary<string, object>> Get(Key key)
        {
            CheckKey(key);
            var (found, result) = await this.TryGet(key).ConfigureAwait(false);
            return found ? result : throw new KeyNotFoundException();
        }

        public async Task<(bool, IDictionary<string, object>)> TryGet(Key key)
        {
            CheckKey(key);

            var query = MakeQueryByKey(key);
            var resultSet = await this.table.ExecuteQuerySegmentedAsync(query, null).ConfigureAwait(false);
            var result = resultSet.Results.FirstOrDefault();
            if (result == null)
                return (false, null);
            return (true, result.Properties.ToDictionary(kv => kv.Key, kv => kv.Value.PropertyAsObject));
        }

        private static TableQuery MakeQueryByKey(Key key)
        {
            return new TableQuery().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.Equal, key.Row),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, key.Partition)));
        }

        public Task Put(Key key, IDictionary<string, object> value)
        {
            CheckKey(key);
            if (value == null)
                throw new ArgumentNullException(nameof(value));


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

        public async Task<VersionedEntry<string, IDictionary<string, object>>> TryGetVersioned(Key key)
        {
            CheckKey(key);

            var query = MakeQueryByKey(key);
            var resultSet = await this.table.ExecuteQuerySegmentedAsync(query, null).ConfigureAwait(false);
            var result = resultSet.Results.FirstOrDefault();
            if (result == null)
                return null;

            return new VersionedEntry<string, IDictionary<string, object>>
            {
                Version = result.ETag,
                Value = result.Properties.ToDictionary(kv => kv.Key, kv => kv.Value.PropertyAsObject),
            };
        }

        public async Task<bool> Put(Key key, IDictionary<string, object> value, string versionToUpdate)
        {
            CheckKey(key);
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var entity = new AzureTableEntity(key, value);
            TableOperation operation;
            if (versionToUpdate == null)
            {
                operation = TableOperation.Insert(entity);
            }
            else
            {
                entity.ETag = versionToUpdate;
                operation = TableOperation.Replace(entity);
            }
            try
            {
                var result = await this.table.ExecuteAsync(operation).ConfigureAwait(false);
                // .NET is bizzare. In my practice, when there's concurrency violation,
                // StorageException is thrown. However, for some reason TableResult includes status code
                // Wonder, why throw if they could just return the status code? It's a common situation...
                return result.HttpStatusCode >= 200 && result.HttpStatusCode < 300;
            }
            catch (StorageException exception) 
                when (exception.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed
                    || exception.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                return false;
            }
        }

        internal static void CheckKey(Key key)
        {
            if (string.IsNullOrEmpty(key.Partition))
                throw new ArgumentNullException(nameof(key.Partition));
            if (string.IsNullOrEmpty(key.Row))
                throw new ArgumentNullException(nameof(key.Row));
        }
    }
}