namespace MiApi.Models
{
    public class Usuario
    {
        public int Id { get; set; }            // Clave primaria
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Passwprd { get; set; } = null!;
        public string Puesto { get; set; } = null!;
        public string Empresa { get; set; } = null!;
    }
}
