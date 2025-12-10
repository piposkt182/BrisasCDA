using Application.Utilities.Interfaces;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Application.Utilities
{
    public class BlobService : IBlobService
    {
        private string connectionString;
        public BlobService(IConfiguration config)
        {
            connectionString = config["Azure:BlobStorage"];
        }
        public async Task<bool> UploadFile(string containerName, IFormFile file)
        {
            BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
            await container.UploadBlobAsync(file.FileName, file.OpenReadStream());
            return true;
        }

        public async Task<string> UploadToAzureAsync(string containerName, byte[] fileBytes, string fileName)
        {
             BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
            await container.CreateIfNotExistsAsync();
            //await container.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob); // Opcional si quieres acceso público

            BlobClient blob = container.GetBlobClient(fileName);

            using var stream = new MemoryStream(fileBytes);
            await blob.UploadAsync(stream, overwrite: true);

            return blob.Uri.ToString(); // URL accesible del archivo
        }

    }
}
