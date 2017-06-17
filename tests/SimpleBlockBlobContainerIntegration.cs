namespace LostTech.Storage
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;

    [TestClass]
    public class SimpleBlockBlobContainerIntegration
    {
        [TestMethod]
        public async Task CanReadBack()
        {
            var container = await GetTestContainer();
            Guid data = Guid.NewGuid();
            await container.Put("test", data.ToByteArray());
            var afterRoundTrip = await container.Get("test");
            Assert.AreEqual(data, new Guid(afterRoundTrip));
        }
        [TestMethod]
        public async Task CanTryReadBack()
        {
            var container = await GetTestContainer();
            Guid data = Guid.NewGuid();
            await container.Put("test", data.ToByteArray());
            var (ok, afterRoundTrip) = await container.TryGet("test");
            Assert.IsTrue(ok);
            Assert.AreEqual(data, new Guid(afterRoundTrip));
        }

        [TestMethod]
        public async Task TryReadBackDoesNotThrowOnNonExistent()
        {
            var container = await GetTestContainer();
            var (ok, _) = await container.TryGet("test");
            Assert.IsFalse(ok);
        }
        static async Task<SimpleBlockBlobContainer> GetTestContainer([CallerMemberName] string containerName = null)
        {
            if (containerName == null)
                throw new ArgumentNullException(nameof(containerName));

            var account = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            var blobClient = account.CreateCloudBlobClient();
            var table = blobClient.GetContainerReference((containerName + "IntegTest").ToLowerInvariant());
            await table.DeleteIfExistsAsync().ConfigureAwait(false);
            await table.CreateAsync().ConfigureAwait(false);
            return new SimpleBlockBlobContainer(table);
        }
    }
}
