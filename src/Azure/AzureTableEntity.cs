namespace LostTech.NKeyValue
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    sealed class AzureTableEntity : ITableEntity
    {
        readonly IDictionary<string, object> value;
        public AzureTableEntity(string rowKey, IDictionary<string, object> value, string partitionKey = null)
        {
            this.RowKey = rowKey ?? throw new ArgumentNullException(nameof(rowKey));
            this.PartitionKey = partitionKey ?? rowKey;
            this.value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string ETag { get; set; } = "*";
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));
            if (operationContext == null)
                throw new ArgumentNullException(nameof(operationContext));

            this.value.Clear();
            foreach(var keyValue in properties)
            {
                this.value.Add(keyValue.Key, keyValue.Value.PropertyAsObject);
            }
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
            => this.value.ToDictionary(kv => kv.Key, kv => EntityProperty.CreateEntityPropertyFromObject(kv.Value));
    }
}
