using Microsoft.AspNetCore.Http;

namespace Application.Utilities.Interfaces
{
    public interface IBlobService
    {
        Task<bool> UploadFile(string container, IFormFile file);
    }
}
