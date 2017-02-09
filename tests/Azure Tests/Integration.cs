namespace LostTech.NKeyValue
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Threading.Tasks;

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
            var original = await table.TryGetTagged(entityKey);

            Assert.IsTrue(await table.Put(entityKey, new Dictionary<string, object>
            {
                ["Key"] = "value1",
            }, tag: original.Tag));
            Assert.IsFalse(await table.Put(entityKey, new Dictionary<string, object>
            {
                ["Key"] = "value2",
            }, tag: original.Tag));
            Assert.AreEqual("value1", (await table.TryGetTagged(entityKey)).Value["Key"]);
        }
    }
}
