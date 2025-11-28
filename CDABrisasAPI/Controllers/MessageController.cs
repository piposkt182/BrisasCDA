using Application.Abstractions.Interfaces.Dispatchers.Interfaz;
using Application.Messages.Commands;
using Application.Messages.Queries;
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

        public MessageController(ILogger<MessageController> logger, IDispatcher dispatcher)
        {
            _logger = logger;
            _dispatcher = dispatcher;
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
    }
}
