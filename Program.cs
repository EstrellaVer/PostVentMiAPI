using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using Microsoft.OpenApi.Models; // <-- necesario para Swagger


var builder = WebApplication.CreateBuilder(args);

// Configurar EF Core con MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Puerto asignado por Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
