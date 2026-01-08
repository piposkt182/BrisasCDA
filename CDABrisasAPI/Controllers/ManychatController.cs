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

        [HttpPost("SaveSurveyUser")]
        public async Task<IResult> SaveSurveyUser([FromHeader(Name = "whatsappnumber")] string whatsappnumber, [FromHeader(Name = "field1")] string field1, [FromHeader(Name = "field2")] string field2, [FromHeader(Name = "field3")] string field3)
        {
            var command = new SaveSurveyUserCommand(whatsappnumber, field1, field2, field3);
            var surveysaved = await _dispatcher.SendCommandAsync<SaveSurveyUserCommand, bool>(command);
            return TypedResults.Ok(surveysaved);
        }
    }
}
