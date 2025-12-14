using Microsoft.AspNetCore.Http;

namespace Application.Utilities.Interfaces
{
    public interface IBlobService
    {
        Task<bool> UploadFile(string container, IFormFile file);
        Task<string> UploadToAzureAsync(string containerName, byte[] fileBytes, string fileName);
        Task<string> GetImageWithSasFromUrl(string imageUrl);
    }
}
