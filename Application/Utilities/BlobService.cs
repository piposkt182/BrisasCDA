using Application.Utilities.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
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

        public async Task<string> GetImageWithSasFromUrl(string imageUrl)
        {
            // 1️⃣ Parsear la URL guardada
            var uri = new Uri(imageUrl);
           
            var segments = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 2)
                throw new ArgumentException("La URL no es una URL válida de Azure Blob.");

            var containerName = segments[0];                 // referidos
            var blobName = string.Join('/', segments.Skip(1)); // prueba

            // 2️⃣ Crear el BlobClient usando la URL original
            var blobClient = new BlobClient(
                connectionString,
                containerName,
                blobName);

            // 3️⃣ Construir SAS
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            // 4️⃣ Generar URL con SAS
            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }


    }
}
