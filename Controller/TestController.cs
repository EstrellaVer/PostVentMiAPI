using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Microsoft.Extensions.Configuration;
using System;

namespace MiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public TestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("test-db")]
        public IActionResult TestDb()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    return Ok("Conexión a DB exitosa!");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error de conexión: {ex.Message}");
            }
        }
    }
}
