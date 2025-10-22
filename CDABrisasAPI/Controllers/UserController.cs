using Application.Abstractions.Interfaces.Dispatchers.Interfaz;
using Application.SystemUsers.Commands;
using Application.SystemUsers.Queries;
using Application.Users.Queries;
using CDABrisasAPI.Dto;
using Domain.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CDABrisasAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("allowCors")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDispatcher _dispatcher;

        public UserController(ILogger<WeatherForecastController> logger, IDispatcher dispatcher)
        {
            _logger = logger;
            _dispatcher = dispatcher;
        }

        [HttpPost("CreateUser")]
        public async Task<IResult> CreateUser([FromBody] SystemUserDto userCreate)
        {
            var command = new CreateSystemUserCommand(userCreate.UserName, userCreate.PasswordHash, userCreate.RoleId);
            var user = await _dispatcher.SendCommandAsync<CreateSystemUserCommand, SystemUser>(command);
            if (user == null)
            {
                return Results.Unauthorized();
            }
            return TypedResults.Ok(user);
        }

        [HttpPost("Login")]
        public async Task<IResult> Login([FromBody] UserLoginDto userLogin)
        {
            var query = new GetSystemUserByUserNameQuery(userLogin.UserName, userLogin.Password);
            var user = await _dispatcher.SendQueryAsync<GetSystemUserByUserNameQuery, SystemUser>(query);
            if (user == null)
            {
                return Results.Unauthorized();
            }
            return TypedResults.Ok(user);
        }

        [HttpGet("GetAllUsersWithMessages")]
        public async Task<IActionResult> GetAllUsersWithMessages()
        {
            var query = new GetAllUsersWithMessagesQuery();
            var users = await _dispatcher.SendQueryAsync<GetAllUsersWithMessagesQuery, IEnumerable<User>>(query);
            return Ok(users);
        }

    }
}

    
