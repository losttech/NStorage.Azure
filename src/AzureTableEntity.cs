namespace LostTech.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Key = PartitionedKey<string, string>;

    public sealed class AzureTableEntity : ITableEntity
    {
        readonly IDictionary<string, object> value;
        public AzureTableEntity(Key key, IDictionary<string, object> value) : this(key)
        {
            this.value = value ?? throw new ArgumentNullException(nameof(value));
        }

        private AzureTableEntity(Key key)
        {
            AzureTable.CheckKey(key);
            this.RowKey = key.Row;
            this.PartitionKey = key.Partition;
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
                if (!IsKey(keyValue.Key))
                    continue;
                
                this.value.Add(DecodeKey(keyValue.Key), keyValue.Value.PropertyAsObject);
            }
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
            => this.value.ToDictionary(kv => EncodeKey(kv.Key), kv => EntityProperty.CreateEntityPropertyFromObject(kv.Value));

        internal static AzureTableEntity KeyOnly(Key key) => new AzureTableEntity(key);

        public static bool IsKey(string key) =>
            string.IsNullOrEmpty(key)
            ? throw new ArgumentNullException(nameof(key))
            : key[0] == '_';

        static Regex UnsupportedPropertyCharRegex = new Regex(@"[^\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}_]", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        static Regex UnsupportedEscapedPropertyCharRegex = new Regex(@"_[a-f0-9]{2}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string EncodeKey(string key) =>
            key == null
            ? throw new ArgumentNullException(nameof(key))
            : "_" + UnsupportedPropertyCharRegex.Replace(key, badMatch => $"_{checked((byte)badMatch.Value[0]):x2}");

        public static string DecodeKey(string key) =>
            string.IsNullOrEmpty(key)
            ? throw new ArgumentNullException(nameof(key))
            : UnsupportedEscapedPropertyCharRegex.Replace(key.Substring(1), badMatch => ((char)Byte.Parse(badMatch.Value.Substring(1), NumberStyles.HexNumber)).ToString());
    }
}
