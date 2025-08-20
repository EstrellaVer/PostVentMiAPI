using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Variables de entorno para la DB
var host = Environment.GetEnvironmentVariable("MYSQL_HOST") ?? "localhost";
var port = Environment.GetEnvironmentVariable("MYSQL_PORT") ?? "3306";
var database = Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? "railway";
var user = Environment.GetEnvironmentVariable("MYSQL_USER") ?? "root";
var password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD") ?? "ttJudGGukEAGCYSzbaEYDVmoqAXLcOLp";

var connectionString = $"Server={host};Port={port};Database={database};User={user};Password={password};";

// Configurar EF Core con MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔹 Aquí usamos UseUrls antes de Build()
var portRailway = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{portRailway}");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MiApi V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
