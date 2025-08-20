using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("🚀 Iniciando aplicación...");

// Variables de entorno para Railway
var host = Environment.GetEnvironmentVariable("MYSQLHOST");
var port = Environment.GetEnvironmentVariable("MYSQLPORT");
var database = Environment.GetEnvironmentVariable("MYSQLDATABASE");
var user = Environment.GetEnvironmentVariable("MYSQLUSER");
var password = Environment.GetEnvironmentVariable("MYSQLPASSWORD");
var portRailway = Environment.GetEnvironmentVariable("PORT") ?? "5000";

Console.WriteLine($"Variables de entorno:");
Console.WriteLine($"- MYSQLHOST: {host ?? "NULL"}");
Console.WriteLine($"- MYSQLPORT: {port ?? "NULL"}");
Console.WriteLine($"- MYSQLDATABASE: {database ?? "NULL"}");
Console.WriteLine($"- MYSQLUSER: {user ?? "NULL"}");
Console.WriteLine($"- MYSQLPASSWORD: {(string.IsNullOrEmpty(password) ? "NULL" : "SET")}");
Console.WriteLine($"- PORT: {portRailway}");

// Fallbacks para desarrollo local
host = host ?? "localhost";
port = port ?? "3306";
database = database ?? "railway";
user = user ?? "root";
password = password ?? "";

var connectionString = $"Server={host};Port={port};Database={database};User={user};Password={password};";

Console.WriteLine($"📡 Connection String: Server={host};Port={port};Database={database};User={user};Password=***");

try
{
    // Configurar EF Core con MySQL
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.LogTo(Console.WriteLine);
        }
    });
    Console.WriteLine("✅ DbContext configurado correctamente");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error configurando DbContext: {ex.Message}");
}

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

// IMPORTANTE: Configurar el puerto para Railway
builder.WebHost.UseUrls($"http://0.0.0.0:{portRailway}");
Console.WriteLine($"🌐 Configurado para escuchar en puerto: {portRailway}");

var app = builder.Build();

Console.WriteLine($"🏗️ Aplicación construida, Entorno: {app.Environment.EnvironmentName}");

// Swagger disponible en todos los entornos para debugging
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MiApi V1");
    c.RoutePrefix = string.Empty; // Swagger en la raíz
});

// Middleware para logging de requests
app.Use(async (context, next) =>
{
    Console.WriteLine($"📥 {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {context.Request.Method} {context.Request.Path}");
    await next();
});

// NO usar HTTPS redirect en Railway
// app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

// Verificar conexión a DB al iniciar
Console.WriteLine("🔍 Verificando conexión a base de datos...");
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        Console.WriteLine("🔄 Intentando conectar a DB...");
        var canConnect = context.Database.CanConnect();
        
        if (canConnect)
        {
            Console.WriteLine("✅ Conexión a DB exitosa!");
            
            // Intentar crear las tablas si no existen
            Console.WriteLine("🔄 Verificando/creando tablas...");
            context.Database.EnsureCreated();
            
            var count = context.Usuarios.Count();
            Console.WriteLine($"📊 Usuarios en DB: {count}");
        }
        else
        {
            Console.WriteLine("❌ No se pudo conectar a la base de datos");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error al verificar DB: {ex.Message}");
    Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
}

// Endpoint simple de health check
app.MapGet("/health", () =>
{
    Console.WriteLine("🏥 Health check solicitado");
    return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
});

// Endpoint raíz
app.MapGet("/", () =>
{
    Console.WriteLine("🏠 Endpoint raíz solicitado");
    return Results.Content("<h1>🚀 API funcionando correctamente</h1><p><a href='/swagger'>Ver Swagger</a></p>", "text/html");
});

Console.WriteLine($"🎯 Aplicación lista para recibir requests en puerto {portRailway}");
Console.WriteLine("📋 Endpoints disponibles:");
Console.WriteLine("  - GET  /");
Console.WriteLine("  - GET  /health");
Console.WriteLine("  - GET  /swagger");
Console.WriteLine("  - GET  /api/usuarios");
Console.WriteLine("  - POST /api/usuarios");
Console.WriteLine("  - POST /api/usuarios/login");

app.Run();