using Application.Abstractions.Interfaces.Dispatchers.Interfaz;
using Application.Messages.Commands;
using Application.Utilities.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CDABrisasAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("allowCors")]
    public class ExcelFileController : ControllerBase
    {
        private readonly IExcelFileService _excelFileService;
        private readonly IDispatcher _dispatcher;
        public ExcelFileController(IExcelFileService excelFileService, IDispatcher dispatcher)
        {
                _excelFileService = excelFileService;
            _dispatcher = dispatcher;
        }

        [HttpPost("UploadExcelAgreement")]
        public async Task<IActionResult> UploadExcelAgreement(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid field");

            var result = await _excelFileService.UploadExcel(file);
            var command = new SetUserForAgreementCommand(result);
            var user = await _dispatcher.SendCommandAsync<SetUserForAgreementCommand, IEnumerable<Message>>(command);
            return Ok(user);
        }
    }
}
