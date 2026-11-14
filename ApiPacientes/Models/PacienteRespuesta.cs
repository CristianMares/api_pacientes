namespace ApiPacientes.Models;

public record PacienteRespuesta(
    int IdPaciente,
    string Nombre,
    string APaterno,
    string AMaterno,
    string Direccion,
    string Telefono,
    double Peso,
    double Altura,
    string Estatus,
    double Imc,
    string CategoriaImc)
{
    public static PacienteRespuesta Desde(Paciente p)
    {
        var metros = p.Altura / 100;
        var imc = metros > 0 ? Math.Round(p.Peso / (metros * metros), 1) : 0;

        return new PacienteRespuesta(
            p.IdPaciente, p.Nombre, p.APaterno, p.AMaterno ?? "", p.Direccion,
            p.Telefono, p.Peso, p.Altura, p.Estatus, imc, Categoria(imc));
    }

    private static string Categoria(double imc) => imc switch
    {
        < 18.5 => "Bajo peso",
        < 25 => "Normal",
        < 30 => "Sobrepeso",
        _ => "Obesidad"
    };
}
