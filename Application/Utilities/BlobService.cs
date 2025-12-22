using Application.Utilities.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Application.Utilities
{
    public class BlobService : IBlobService
    {
        private readonly string _connectionString;

        public BlobService(IConfiguration config)
        {
            _connectionString = config["Azure:BlobStorage"];
        }

        // 🔹 MÉTODO CENTRALIZADO (CLAVE)
        private BlobContainerClient GetContainerClient(string containerName)
        {
            return new BlobContainerClient(_connectionString, containerName);
        }

        public async Task<bool> UploadFile(string containerName, IFormFile file)
        {
            var container = GetContainerClient(containerName);
            await container.CreateIfNotExistsAsync();

            await container.UploadBlobAsync(file.FileName, file.OpenReadStream());
            return true;
        }

        public async Task<string> UploadToAzureAsync(string containerName, byte[] fileBytes, string fileName)
        {
            var container = GetContainerClient(containerName);
            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlobClient(fileName);

            using var stream = new MemoryStream(fileBytes);
            await blob.UploadAsync(stream, overwrite: true);

            return blob.Uri.ToString(); // se guarda en DB
        }

        public async Task<string> GetImageWithSasFromUrl(string imageUrl)
        {
            var uri = new Uri(imageUrl);

            var segments = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 2)
                throw new ArgumentException("La URL no es una URL válida de Azure Blob.");

            var containerName = segments[0];
            var blobName = string.Join('/', segments.Skip(1));

            var container = GetContainerClient(containerName);
            var blobClient = container.GetBlobClient(blobName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        public async Task<(Stream Stream, string ContentType)?> OpenReadStreamAsync(
            string blobName,
            CancellationToken ct = default)
        {
            var container = GetContainerClient("referidos");
            var blobClient = container.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync(ct))
                return null;

            var props = await blobClient.GetPropertiesAsync(cancellationToken: ct);
            var stream = await blobClient.OpenReadAsync(cancellationToken: ct);

            return (stream, props.Value.ContentType ?? "application/octet-stream");
        }
    }
}
