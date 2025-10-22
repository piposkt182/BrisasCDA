using Application.Abstractions.Interfaces.Dispatchers.Interfaz;
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
    }
}
