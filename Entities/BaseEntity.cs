namespace ClinicaAmelita.Api.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public int? UsuarioCreacionId { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public int? UsuarioActualizacionId { get; set; }
    public bool EsActivo { get; set; } = true;
}
