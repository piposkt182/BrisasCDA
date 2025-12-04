using Application.Abstractions.Interfaces.Dispatchers.Interfaz;
using Application.Messages.Commands;
using Application.SystemUsers.CommandHandler;
using Application.SystemUsers.Commands;
using Application.Users.Commands;
using Application.Users.Queries;
using Application.Utilities.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace CDABrisasAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDispatcher _dispatcher;
        private readonly IWhatsAppWebhookParser _parser;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDispatcher dispatcher, IWhatsAppWebhookParser parser)
        {
            _logger = logger;
            _dispatcher = dispatcher;
            _parser = parser;
        }

        [HttpGet]
        public IActionResult Verify(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.verify_token")] string verifyToken,
            [FromQuery(Name = "hub.challenge")] string challenge)
        {
            const string VERIFY_TOKEN = "blink182"; // Debe coincidir con el que pusiste en Meta

            if (mode == "subscribe" && verifyToken == VERIFY_TOKEN)
            {
                return Content(challenge);
            }

            return Unauthorized();
        }

        [HttpPost]
        public async Task<IResult> ReceiveMessage([FromBody] object payload)
        {
            User newUserId = new User();
            var contactInfo = _parser.ExtractContactInfo(payload.ToString());
            if (contactInfo == null)
                return Results.BadRequest("No se pudieron obtener los datos del contacto.");

            var (name, wsId, text, dateMessage, mimeType, mediaId, typeMessage) = contactInfo.Value;
            if(typeMessage == 3)
            {
                var querySystemUser = new SendReferralListCommand("573012282168");
                var user = await _dispatcher.SendCommandAsync<SendReferralListCommand, Message>(querySystemUser);
            }
            else
            {
                var query = new GetUserQuery(wsId);
                var user = await _dispatcher.SendQueryAsync<GetUserQuery, User>(query);
                if (user == null)
                {
                    var command = new CreateUserCommand(name, wsId);
                    newUserId = await _dispatcher.SendCommandAsync<CreateUserCommand, User>(command);
                }else
                    newUserId.Id=user.Id;

                var commandMsg = new CreateMessageCommand(newUserId.Id, wsId, text!, dateMessage, mimeType, mediaId, typeMessage);
                var message = await _dispatcher.SendCommandAsync<CreateMessageCommand, Message>(commandMsg);
            }
            _logger.LogInformation("Mensaje recibido: " + payload.ToString());
            return TypedResults.Ok(newUserId);
        }
    }
}
