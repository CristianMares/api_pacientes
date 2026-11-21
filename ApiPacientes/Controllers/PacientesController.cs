using ApiPacientes.Data;
using ApiPacientes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiPacientes.Controllers;

[ApiController]
[Route("api/pacientes")]
public class PacientesController(PacientesContext db) : ControllerBase
{
    // GET api/pacientes?buscar=juan&estatus=A
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PacienteRespuesta>>> Listar(string? buscar, string? estatus)
    {
        var query = db.Pacientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(estatus))
        {
            var e = estatus.Trim().ToUpper();
            query = query.Where(p => p.Estatus == e);
        }

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim().ToLower();
            query = query.Where(p =>
                (p.Nombre + " " + p.APaterno + " " + (p.AMaterno ?? "")).ToLower().Contains(texto));
        }

        var lista = await query.OrderBy(p => p.IdPaciente).ToListAsync();
        return Ok(lista.Select(PacienteRespuesta.Desde));
    }

    // GET api/pacientes/duplicado?nombre=..&aPaterno=..&aMaterno=..&telefono=..&excluirId=..
    // Busca otro paciente con el mismo nombre completo y teléfono
    [HttpGet("duplicado")]
    public async Task<ActionResult<object>> Duplicado(string nombre, string aPaterno, string? aMaterno, string telefono, int? excluirId)
    {
        var tel = (telefono ?? "").Trim();
        var buscado = NombreNormalizado(nombre, aPaterno, aMaterno);

        var candidatos = await db.Pacientes.AsNoTracking()
            .Where(p => p.Telefono == tel && (excluirId == null || p.IdPaciente != excluirId))
            .ToListAsync();

        var encontrado = candidatos.FirstOrDefault(p => NombreNormalizado(p.Nombre, p.APaterno, p.AMaterno) == buscado);

        return Ok(new
        {
            existe = encontrado is not null,
            paciente = encontrado is null ? null : PacienteRespuesta.Desde(encontrado)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PacienteRespuesta>> Obtener(int id)
    {
        var p = await db.Pacientes.FindAsync(id);
        return p is null ? NotFound(new { mensaje = "Paciente no encontrado" }) : Ok(PacienteRespuesta.Desde(p));
    }

    [HttpPost]
    public async Task<ActionResult<PacienteRespuesta>> Crear(Paciente p)
    {
        // El Id lo asigna la API: siguiente al mayor existente
        p.IdPaciente = (await db.Pacientes.MaxAsync(x => (int?)x.IdPaciente) ?? 0) + 1;

        Limpiar(p);
        p.Estatus = "A";
        p.FechaRegistro = DateTime.UtcNow;
        p.FechaModificacion = null;
        db.Pacientes.Add(p);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Obtener), new { id = p.IdPaciente }, PacienteRespuesta.Desde(p));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PacienteRespuesta>> Editar(int id, Paciente datos)
    {
        if (id != datos.IdPaciente)
            return BadRequest(new { mensaje = "El Id no coincide" });

        var p = await db.Pacientes.FindAsync(id);
        if (p is null) return NotFound(new { mensaje = "Paciente no encontrado" });

        Limpiar(datos);
        p.Nombre = datos.Nombre;
        p.APaterno = datos.APaterno;
        p.AMaterno = datos.AMaterno;
        p.Direccion = datos.Direccion;
        p.Telefono = datos.Telefono;
        p.Peso = datos.Peso;
        p.Altura = datos.Altura;
        p.FechaModificacion = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Ok(PacienteRespuesta.Desde(p));
    }

    [HttpPatch("{id:int}/cancelar")]
    public Task<ActionResult<PacienteRespuesta>> Cancelar(int id) => CambiarEstatus(id, "C");

    [HttpPatch("{id:int}/reactivar")]
    public Task<ActionResult<PacienteRespuesta>> Reactivar(int id) => CambiarEstatus(id, "A");

    private async Task<ActionResult<PacienteRespuesta>> CambiarEstatus(int id, string estatus)
    {
        var p = await db.Pacientes.FindAsync(id);
        if (p is null) return NotFound(new { mensaje = "Paciente no encontrado" });

        p.Estatus = estatus;
        p.FechaModificacion = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(PacienteRespuesta.Desde(p));
    }

    private static string NombreNormalizado(string? n, string? ap, string? am) =>
        string.Join(' ', $"{n} {ap} {am}".ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static void Limpiar(Paciente p)
    {
        p.Nombre = p.Nombre.Trim();
        p.APaterno = p.APaterno.Trim();
        p.AMaterno = (p.AMaterno ?? "").Trim();
        p.Direccion = p.Direccion.Trim();
        p.Telefono = p.Telefono.Trim();
    }
}
