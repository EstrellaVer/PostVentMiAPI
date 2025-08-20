using Microsoft.AspNetCore.Mvc;
using MiApi.Data;
using MiApi.Models;

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

        // Endpoint para probar conexión a DB
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

        // Endpoint para guardar un usuario de prueba
        [HttpPost("test-save")]
        public IActionResult TestSave()
        {
            try
            {
                var usuario = new Usuario
                {
                    Nombre = "Prueba",
                    Email = "prueba@email.com",
                    Password = "1234",
                    Puesto = "Tester",
                    Empresa = "MiEmpresa"
                };

                _context.Usuarios.Add(usuario);
                _context.SaveChanges(); // Muy importante para guardar en DB

                return Ok($"Usuario guardado correctamente con ID: {usuario.Id}");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al guardar usuario: {ex.Message}");
            }
        }

        // Endpoint raíz para ver en navegador
        [HttpGet("/")]
        public IActionResult Home()
        {
            return Content("<h1>API funcionando correctamente</h1>", "text/html");
        }
    }
}
