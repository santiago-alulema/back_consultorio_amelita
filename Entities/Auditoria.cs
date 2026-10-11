namespace ClinicaAmelita.Api.Entities;

public class Auditoria
{
    public long Id { get; set; }
    public string Entidad { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string RegistroId { get; set; } = string.Empty;
    public int? UsuarioId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? Datos { get; set; }
}
