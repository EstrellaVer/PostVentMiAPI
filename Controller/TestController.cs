using Microsoft.AspNetCore.Mvc;
using MiApi.Data;

namespace MiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("test-db")]
        public IActionResult TestDb()
        {
            try
            {
                var count = _context.Usuarios.Count();
                return Ok($"Conexión a DB exitosa! Usuarios en la tabla: {count}");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error de conexión: {ex.Message}");
            }
        }

        [HttpGet("/")]
        public IActionResult Home()
        {
            return Content("<h1>API funcionando correctamente</h1>", "text/html");
        }
    }
}
