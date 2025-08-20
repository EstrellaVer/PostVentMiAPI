using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Variables de entorno correctas para Railway
var host = Environment.GetEnvironmentVariable("MYSQLHOST") ?? "localhost";
var port = Environment.GetEnvironmentVariable("MYSQLPORT") ?? "3306";
var database = Environment.GetEnvironmentVariable("MYSQLDATABASE") ?? "railway";
var user = Environment.GetEnvironmentVariable("MYSQLUSER") ?? "root";
var password = Environment.GetEnvironmentVariable("MYSQLPASSWORD") ?? "";

var connectionString = $"Server={host};Port={port};Database={database};User={user};Password={password};";

Console.WriteLine($"Conexión DB: Server={host};Port={port};Database={database};User={user}");

// Configurar EF Core con MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    // Opcional: Habilitar logging sensible para development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MiApi",
        Version = "v1"
    });
});

// Configurar logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

// Puerto para Railway
var portRailway = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{portRailway}");

var app = builder.Build();

// Configurar Swagger para todos los entornos (útil para debug en Railway)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MiApi V1");
    c.RoutePrefix = string.Empty;
});

// Middleware para logging de requests
app.Use(async (context, next) =>
{
    Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {context.Request.Method} {context.Request.Path}");
    await next();
});

// NO uses UseHttpsRedirection en Railway a menos que tengas SSL configurado
// app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

// Verificar conexión a DB al iniciar
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var canConnect = context.Database.CanConnect();
        Console.WriteLine($"Conexión a DB: {(canConnect ? "✅ Exitosa" : "❌ Fallida")}");
        
        if (canConnect)
        {
            var count = context.Usuarios.Count();
            Console.WriteLine($"Usuarios en DB: {count}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error al conectar con DB: {ex.Message}");
}

Console.WriteLine($"🚀 API iniciada en puerto {portRailway}");
app.Run();