using Application.Abstractions.Interfaces.Dispatchers.Interfaz;
using Application.Messages.Commands;
using Application.Messages.Queries;
using Application.Utilities.Interfaces;
using Azure.Identity;
using Domain.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CDABrisasAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    [EnableCors("allowCors")]
    public class MessageController : ControllerBase
    {
        private readonly ILogger<MessageController> _logger;
        private readonly IDispatcher _dispatcher;
        private readonly IBlobService _blobService;

        public MessageController(ILogger<MessageController> logger, IDispatcher dispatcher, IBlobService blobService)
        {
            _logger = logger;
            _dispatcher = dispatcher;
            _blobService = blobService;
        }

        [HttpGet("GetAllMessages")]
        public async Task<IActionResult> GetAllMessages()
        {
            var query = new GetAllMessagesQuery();
            var messages = await _dispatcher.SendQueryAsync<GetAllMessagesQuery, IEnumerable<Message>>(query);
            return Ok(messages);
        }

        [HttpGet("SendWhatsAppTemplate")]
        public async Task<IActionResult> SendWhatsAppTemplate(string subscriberId, string name, string code, string plate, string plateDate)
        {
            var command = new SendWhatsAppTemplateCommand(subscriberId, name, plate, plateDate);
            var messages = await _dispatcher.SendCommandAsync<SendWhatsAppTemplateCommand, string>(command);
            return Ok(messages);
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            await _blobService.UploadFile("referidos", file);
            return StatusCode(200, "Imagen guardada");
        }
    }
}
