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
    }
}
