using Microsoft.AspNetCore.Http;

namespace Application.Utilities.Interfaces
{
    public interface IExcelFileService
    {
        Task<List<Dictionary<string, string>>> UploadExcel(IFormFile file);
    }
}
