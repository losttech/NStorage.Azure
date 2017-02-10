namespace LostTech.NKeyValue
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Integration
    {
        [TestMethod]
        public async Task ConcurrentUpdate()
        {
            var table = await AzureTable.OpenOrCreate("UseDevelopmentStorage=true", "test");
            string entityKey = nameof(this.ConcurrentUpdate);
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
    }
}
