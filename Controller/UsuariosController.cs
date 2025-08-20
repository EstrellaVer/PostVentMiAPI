using Microsoft.AspNetCore.Mvc;
using MiApi.Data;
using MiApi.Models;
using System.ComponentModel.DataAnnotations;

namespace MiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(AppDbContext context, ILogger<UsuariosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetUsuarios()
        {
            try
            {
                var usuarios = _context.Usuarios.ToList();
                _logger.LogInformation($"Se obtuvieron {usuarios.Count} usuarios");
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener usuarios: {ex.Message}");
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult CrearUsuario([FromBody] Usuario usuario)
        {
            try
            {
                _logger.LogInformation($"Intentando crear usuario: {usuario?.Email}");
                
                // Validar que el usuario no sea null
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario recibido es null");
                    return BadRequest("Los datos del usuario son requeridos");
                }

                // Validar campos requeridos
                if (string.IsNullOrEmpty(usuario.Nombre) || 
                    string.IsNullOrEmpty(usuario.Email) || 
                    string.IsNullOrEmpty(usuario.Password))
                {
                    _logger.LogWarning("Campos requeridos faltantes");
                    return BadRequest("Nombre, Email y Password son requeridos");
                }

                // Verificar si el email ya existe
                if (_context.Usuarios.Any(u => u.Email == usuario.Email))
                {
                    _logger.LogWarning($"Email ya existe: {usuario.Email}");
                    return Conflict("El email ya está registrado");
                }

                // Agregar usuario
                _context.Usuarios.Add(usuario);
                var result = _context.SaveChanges();
                
                _logger.LogInformation($"Usuario creado exitosamente con ID: {usuario.Id}, Filas afectadas: {result}");
                
                return CreatedAtAction(nameof(GetUsuarios), new { id = usuario.Id }, usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear usuario: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, $"Error al crear usuario: {ex.Message}");
            }
        }

        // Endpoint adicional para debug
        [HttpGet("test-connection")]
        public IActionResult TestConnection()
        {
            try
            {
                var canConnect = _context.Database.CanConnect();
                var count = _context.Usuarios.Count();
                
                return Ok(new { 
                    CanConnect = canConnect, 
                    TotalUsuarios = count,
                    Message = "Conexión exitosa" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error de conexión: {ex.Message}");
            }
        }
    }
}