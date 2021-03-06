﻿namespace LostTech.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
            var newEntry = await table.TryGetVersioned(entityKey);
            Assert.AreEqual("value1", newEntry.Value["Key"]);
        }

        private static async Task<AzureTable> GetTestTable([CallerMemberName] string tableName = null)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            var account = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            var tableClient = account.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName + "IntegTest");
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

        [TestMethod]
        public async Task KeysAreEncoded()
        {
            var table = await GetTestTable();
            const string disallowedCharacters = "\\/#?а";
            var key = new Key(disallowedCharacters, disallowedCharacters);
            await table.Put(key, new Dict {[disallowedCharacters] = disallowedCharacters});
            var roundTrip = await table.Get(key);
            Assert.AreEqual(disallowedCharacters, roundTrip[disallowedCharacters]);
        }

        [TestMethod]
        public async Task Querying()
        {
            var table = await GetTestTable();
            var key = PartitionedKey.Of(nameof(this.Querying));
            var aToZ = new Range<string>("A", "Z");
            var aToS = new Range<string>("A", "M");
            var exactRange = Range.SingleElement(nameof(this.Querying));

            await table.Put(key, new Dict {});

            var rowRangeWithResult = await table.Query(partitionRange: exactRange, rowRange: aToZ);
            var partitionRangeWithResult = await table.Query(partitionRange: aToZ, rowRange: exactRange);
            var rowRangeWithoutResult = await table.Query(partitionRange: exactRange, rowRange: aToS);
            var partitionRangeWithoutResult = await table.Query(partitionRange: aToS, rowRange: exactRange);
            Assert.AreEqual(2, rowRangeWithResult.Results.Count + partitionRangeWithResult.Results.Count);
            Assert.AreEqual(0, rowRangeWithoutResult.Results.Count + partitionRangeWithoutResult.Results.Count);
        }

        [TestMethod]
        public async Task Continuation()
        {
            var table = await GetTestTable();
            var partition = nameof(this.Continuation);
            foreach(var i in Enumerable.Range(0, 2))
                await table.Put(new Key(partition: partition, row: i.ToString()), new Dict{["val"]=i});
            var exactPartition = Range.SingleElement(partition);
            var rowRange = new Range<string>("0", "1");
            var item1 = await table.Query(partitionRange: exactPartition, rowRange: rowRange, pageSize: 1);
            Assert.IsNotNull(item1.NextPageToken);
            var item2 = await table.Query(partitionRange: exactPartition, rowRange: rowRange, pageSize: 1,
                continuationToken: item1.NextPageToken);
            Assert.AreEqual(0, item1.Results.Single().Value["val"]);
            Assert.AreEqual(1, item2.Results.Single().Value["val"]);
        }
    }
}
