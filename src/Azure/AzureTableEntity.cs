namespace LostTech.NKeyValue
{
    using System;
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    sealed class AzureTableEntity : ITableEntity
    {
        readonly IDictionary<string, object> value;
        public AzureTableEntity(IDictionary<string, object> value)
        {
            this.value = value ?? throw new ArgumentNullException(nameof(value));
        }
        public string ETag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string PartitionKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RowKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTimeOffset Timestamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            throw new NotImplementedException();
        }
    }
}
