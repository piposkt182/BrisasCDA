using Application.Abstractions.Interfaces.Dispatchers.Interfaz;
using Application.Agreements.Query;
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
    public class AgreementController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDispatcher _dispatcher;
        public AgreementController(ILogger<WeatherForecastController> logger, IDispatcher dispatcher)
        {
            _logger = logger;
            _dispatcher = dispatcher;
        }

        [HttpGet("GetAllAgreements")]
        public async Task<IActionResult> GetAllAgreements()
        {
            var query = new GetAllAgreementsQuery();
            var agreements = await _dispatcher.SendQueryAsync<GetAllAgreementsQuery, IEnumerable<Agreement>>(query);
            return Ok(agreements);
        }
    }
}
