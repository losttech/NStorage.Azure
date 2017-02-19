namespace LostTech.Storage
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AzureTableIntegration
    {
        [TestMethod]
        public async Task ConcurrentUpdate()
        {
            var table = await GetTestTable();
            var entityKey = PartitionedKey.Of(nameof(this.ConcurrentUpdate));
            await table.Put(entityKey, new Dictionary<string, object>
            {
                ["Key"] = "value0",
            });
            var original = await table.TryGetVersioned(entityKey);

            Assert.IsTrue(await table.Put(entityKey, new Dictionary<string, object>
            {
                ["Key"] = "value1",
            }, versionToUpdate: original.Version));
            Assert.IsFalse(await table.Put(entityKey, new Dictionary<string, object>
            {
                ["Key"] = "value2",
            }, versionToUpdate: original.Version));
            Assert.AreEqual("value1", (await table.TryGetVersioned(entityKey)).Value["Key"]);
        }

        private static Task<AzureTable> GetTestTable() 
            => AzureTable.OpenOrCreate("UseDevelopmentStorage=true", nameof(AzureTableIntegration) + "test");

        [TestMethod]
        public async Task ConcurrentInsert()
        {
            var table = await GetTestTable();
            var entityKey = PartitionedKey.Of(nameof(this.ConcurrentInsert));
            await table.Put(entityKey, new Dictionary<string, object> {["Key"] = "value0"});
            Assert.IsFalse(await table.Put(entityKey, new Dictionary<string, object>{["Key"] = "value1"}, null));
        }
    }
}
