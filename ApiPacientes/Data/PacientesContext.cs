using ApiPacientes.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiPacientes.Data;

public class PacientesContext(DbContextOptions<PacientesContext> options) : DbContext(options)
{
    public DbSet<Paciente> Pacientes => Set<Paciente>();
}
