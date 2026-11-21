using Microsoft.EntityFrameworkCore;

namespace ApiPacientes.Data;

// EnsureCreated no modifica una base que ya existe,
// aquí se agregan las columnas nuevas si faltan.
public static class Migraciones
{
    public static void Aplicar(PacientesContext db)
    {
        var columnas = new List<string>();
        var con = db.Database.GetDbConnection();
        con.Open();
        try
        {
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PRAGMA table_info(catPacientes)";
            using var r = cmd.ExecuteReader();
            while (r.Read()) columnas.Add(r.GetString(1));
        }
        finally
        {
            con.Close();
        }

        foreach (var col in new[] { "FechaRegistro", "FechaModificacion" })
        {
            if (!columnas.Contains(col))
                db.Database.ExecuteSqlRaw($"ALTER TABLE catPacientes ADD COLUMN {col} TEXT NULL");
        }
    }
}
