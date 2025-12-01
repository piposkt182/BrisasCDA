using Application.Abstractions.Interfaces.Dispatchers.Interfaz;
using Application.Manychat.Command;
using Application.Utilities.Dtos;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CDABrisasAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("allowCors")]
    public class ManychatController : ControllerBase
    {
        private readonly ILogger<MessageController> _logger;
        private readonly IDispatcher _dispatcher;

        public ManychatController(ILogger<MessageController> logger, IDispatcher dispatcher)
        {
            _logger = logger;
            _dispatcher = dispatcher;
        }


        [HttpPost("SetSurveyFieldsToUsers")]
        public async Task<IResult> SetSurveyFieldsToUsers([FromBody] int data)
        {
            var query = new SetSurveyFieldsToUsersCommand(data);
            var user = await _dispatcher.SendCommandAsync<SetSurveyFieldsToUsersCommand, ManyChatUserResponse>(query);
            if (user == null)
            {
                return Results.Unauthorized();
            }
            return TypedResults.Ok(user);
        }
    }
}
