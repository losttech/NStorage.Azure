namespace LostTech.Storage
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Blob;

    public class SimpleBlockBlobContainer : IWriteableKeyValueStore<string, byte[]>
    {
        public CloudBlobContainer Container { get; }
        public SimpleBlockBlobContainer(CloudBlobContainer container)
        {
            this.Container = container ?? throw new ArgumentNullException(nameof(container));
        }
        public async Task<bool?> Delete(string key) =>
            await this.Container.GetBlobReference(KeyEncode(key)).DeleteIfExistsAsync().ConfigureAwait(false);

        public async Task<byte[]> Get(string key)
        {
            CloudBlob blob = this.Container.GetBlobReference(KeyEncode(key));
            using (var memoryStream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(memoryStream).ConfigureAwait(false);
                return memoryStream.ToArray();
            }
        }

        public async Task Put(string key, byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            CloudBlockBlob blob = this.Container.GetBlockBlobReference(KeyEncode(key));
            await blob.UploadFromByteArrayAsync(value, 0, value.Length).ConfigureAwait(false);
            if (this.SetBlobProperties(blob))
                await blob.SetPropertiesAsync().ConfigureAwait(false);
        }

        protected virtual bool SetBlobProperties(CloudBlockBlob blob) => false;

        public async Task<(bool, byte[])> TryGet(string key)
        {
            CloudBlob blob = this.Container.GetBlobReference(KeyEncode(key));
            if (!await blob.ExistsAsync().ConfigureAwait(false))
                return (false, null);
            using (var memoryStream = new MemoryStream()) {
                await blob.DownloadToStreamAsync(memoryStream).ConfigureAwait(false);
                return (true, memoryStream.ToArray());
            }
        }

        public static string KeyEncode(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            // TODO: ENCODING
            // TODO: ENCODING TESTS
            return key;
        }
    }
}
