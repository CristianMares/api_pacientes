using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPacientes.Models;

[Table("catPacientes")]
public class Paciente
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Range(1, int.MaxValue, ErrorMessage = "El Id debe ser mayor a 0")]
    public int IdPaciente { get; set; }

    [Required, StringLength(60)]
    public string Nombre { get; set; } = "";

    [Required, StringLength(60)]
    public string APaterno { get; set; } = "";

    [StringLength(60)]
    public string? AMaterno { get; set; }

    [Required, StringLength(200)]
    public string Direccion { get; set; } = "";

    [Required, RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe tener 10 dígitos")]
    public string Telefono { get; set; } = "";

    // kg
    [Range(1, 500)]
    public double Peso { get; set; }

    // cm
    [Range(30, 280)]
    public double Altura { get; set; }

    [RegularExpression("^[AC]$", ErrorMessage = "Estatus debe ser A o C")]
    public string Estatus { get; set; } = "A";
}
