namespace LostTech.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Key = PartitionedKey<string, string>;
    using Value = System.Collections.Generic.IDictionary<string, object>;
    using Partition = System.String;
    using Row = System.String;
    using Version = System.String;
    using Dict = System.Collections.Generic.Dictionary<string, object>;
    using Microsoft.WindowsAzure.Storage;

    [TestClass]
    public class AzureTableIntegration
    {
        [TestMethod]
        public async Task ConcurrentUpdate()
        {
            var table = await GetTestTable();
            var entityKey = PartitionedKey.Of(nameof(this.ConcurrentUpdate));
            await table.Put(entityKey, new Dict { ["Key"] = "value0" });
            var original = await table.TryGetVersioned(entityKey);

            Assert.IsTrue((await table.Put(entityKey, new Dict { ["Key"] = "value1" }, versionToUpdate: original.Version)).Item1);
            Assert.IsFalse((await table.Put(entityKey, new Dict { ["Key"] = "value2" }, versionToUpdate: original.Version)).Item1);
            Assert.AreEqual("value1", (await table.TryGetVersioned(entityKey)).Value["Key"]);
        }

        private static async Task<AzureTable> GetTestTable([CallerMemberName] string tableName = null)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            var account = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            var tableClient = account.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            await table.DeleteIfExistsAsync().ConfigureAwait(false);
            await table.CreateAsync().ConfigureAwait(false);
            return new AzureTable(table);
        }

        [TestMethod]
        public async Task ConcurrentInsert()
        {
            var table = await GetTestTable();
            var entityKey = PartitionedKey.Of(nameof(this.ConcurrentInsert));
            await table.Put(entityKey, new Dict {["Key"] = "value0"});
            Assert.IsFalse((await table.Put(entityKey, new Dict{["Key"] = "value1"}, null)).Item1);
        }

        [TestMethod]
        public async Task ConcurrentBatchFail()
        {
            var table = await GetTestTable();
            var partition = nameof(this.ConcurrentBatchFail);
            var key1 = new Key(partition, row: "0");
            var key2 = new Key(partition, row: "1");
            await table.Put(key1, new Dict {["Key"] = "value0"});
            var original = await table.TryGetVersioned(key1);

            var putResult = await table.Put(partition: partition, entities: new[] {
                new KeyValuePair<Row, VersionedEntry<Version, Value>>(key2.Row,
                    new VersionedEntry<Version, Value> {
                        Value = new Dict{["Key"] = "value1"},
                    }),
                 new KeyValuePair<Row, VersionedEntry<Version, Value>>(key1.Row,
                    new VersionedEntry<Version, Value> {
                        Value = new Dict{["Key"] = "value1"},
                        // note: current version is omitted, so this should fail
                    }),
            });
            Assert.IsFalse(putResult);
            Assert.IsNull(await table.TryGetVersioned(key2));
        }

        [TestMethod]
        public async Task ConcurrentBatchSuccess() {
            var table = await GetTestTable();
            var partition = nameof(this.ConcurrentBatchSuccess);
            var key1 = new Key(partition, row: "0");
            var key2 = new Key(partition, row: "1");
            await table.Put(key1, new Dict { ["Key"] = "value0" });
            var original = await table.TryGetVersioned(key1);

            var putResult = await table.Put(partition: partition, entities: new[] {
                new KeyValuePair<Row, VersionedEntry<Version, Value>>(key2.Row,
                    new VersionedEntry<Version, Value> {
                        Value = new Dict{["Key"] = "value1"},
                    }),
                 new KeyValuePair<Row, VersionedEntry<Version, Value>>(key1.Row,
                    new VersionedEntry<Version, Value> {
                        Value = new Dict{["Key"] = "value1"},
                        Version = original.Version,
                    }),
            });
            Assert.IsTrue(putResult);
            Assert.IsNotNull(await table.TryGetVersioned(key2));
        }
    }
}
