using ApiPacientes.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// La base vive en App_Data junto a la API
var carpetaDatos = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(carpetaDatos);
var rutaDb = Path.Combine(carpetaDatos, "pacientes.db");

builder.Services.AddDbContext<PacientesContext>(o => o.UseSqlite($"Data Source={rutaDb}"));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PacientesContext>();
    db.Database.EnsureCreated();
    Migraciones.Aplicar(db);
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
