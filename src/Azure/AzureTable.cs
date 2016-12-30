namespace LostTech.NKeyValue
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public sealed class AzureTable: IWriteableKeyValueStore<string, IDictionary<string, object>>
    {
        readonly CloudTable table;

        private AzureTable(CloudTable table)
        {
            this.table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public async Task<IDictionary<string, object>> Get(string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            
            var query = new TableQuery().Where(TableQuery.GenerateFilterCondition("ID", QueryComparisons.Equal, key));
            var resultSet = await this.table.ExecuteQuerySegmentedAsync(query, null).ConfigureAwait(false);
            var result = resultSet.Results.FirstOrDefault();
            return result.Properties.ToDictionary(kv => kv.Key, kv => kv.Value.PropertyAsObject);
        }

        public Task Put(string key, IDictionary<string, object> value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));


            var entity = new AzureTableEntity(value);
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
    }
}