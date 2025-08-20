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
                return StatusCode(500, new { message = $"Error interno: {ex.Message}", success = false });
            }
        }

        [HttpPost]
        public IActionResult CrearUsuario([FromBody] Usuario usuario)
        {
            try
            {
                _logger.LogInformation($"📥 Datos recibidos para registro: {System.Text.Json.JsonSerializer.Serialize(usuario)}");
                
                // Validar que el usuario no sea null
                if (usuario == null)
                {
                    _logger.LogWarning("❌ Usuario recibido es null");
                    return BadRequest(new { message = "Los datos del usuario son requeridos", success = false });
                }

                // Log individual de campos
                _logger.LogInformation($"📋 Validando campos:");
                _logger.LogInformation($"  - Nombre: '{usuario.Nombre}' (Empty: {string.IsNullOrEmpty(usuario.Nombre)})");
                _logger.LogInformation($"  - Email: '{usuario.Email}' (Empty: {string.IsNullOrEmpty(usuario.Email)})");
                _logger.LogInformation($"  - Password: '{usuario.Password}' (Empty: {string.IsNullOrEmpty(usuario.Password)})");
                _logger.LogInformation($"  - Puesto: '{usuario.Puesto}' (Empty: {string.IsNullOrEmpty(usuario.Puesto)})");
                _logger.LogInformation($"  - Empresa: '{usuario.Empresa}' (Empty: {string.IsNullOrEmpty(usuario.Empresa)})");

                // Validar campos requeridos
                if (string.IsNullOrEmpty(usuario.Nombre) || 
                    string.IsNullOrEmpty(usuario.Email) || 
                    string.IsNullOrEmpty(usuario.Password))
                {
                    _logger.LogWarning("❌ Campos requeridos faltantes");
                    return BadRequest(new { message = "Nombre, Email y Password son requeridos", success = false });
                }

                // Verificar si el email ya existe
                var existeEmail = _context.Usuarios.Any(u => u.Email == usuario.Email);
                if (existeEmail)
                {
                    _logger.LogWarning($"❌ Email ya existe: {usuario.Email}");
                    return Conflict(new { message = "El email ya está registrado", success = false });
                }

                // Agregar usuario
                _context.Usuarios.Add(usuario);
                var result = _context.SaveChanges();
                
                _logger.LogInformation($"✅ Usuario creado exitosamente con ID: {usuario.Id}, Filas afectadas: {result}");
                
                return Ok(new { 
                    message = "Usuario registrado exitosamente", 
                    success = true,
                    id = usuario.Id,
                    user = usuario
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al crear usuario: {ex.Message}");
                _logger.LogError($"❌ Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = $"Error al crear usuario: {ex.Message}", success = false });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                _logger.LogInformation($"🔐 Intento de login para: {loginRequest?.Email}");
                
                // Validar que el request no sea null
                if (loginRequest == null)
                {
                    _logger.LogWarning("❌ LoginRequest recibido es null");
                    return BadRequest(new { message = "Datos de login requeridos", success = false });
                }

                // Validar campos requeridos
                if (string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
                {
                    _logger.LogWarning("❌ Email o password faltantes");
                    return BadRequest(new { message = "Email y contraseña son requeridos", success = false });
                }

                // Buscar usuario por email y password
                var usuario = _context.Usuarios.FirstOrDefault(u => 
                    u.Email == loginRequest.Email && u.Password == loginRequest.Password);

                if (usuario == null)
                {
                    _logger.LogWarning($"❌ Usuario no encontrado o contraseña incorrecta para: {loginRequest.Email}");
                    return Unauthorized(new { message = "Credenciales incorrectas", success = false });
                }

                _logger.LogInformation($"✅ Login exitoso para usuario: {usuario.Email}");
                
                return Ok(new { 
                    message = "Login exitoso", 
                    success = true,
                    usuario = usuario
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error en login: {ex.Message}");
                return StatusCode(500, new { message = $"Error en login: {ex.Message}", success = false });
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
                    Message = "Conexión exitosa",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = $"Error de conexión: {ex.Message}", 
                    success = false 
                });
            }
        }
    }

    // Clase para login request
    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}