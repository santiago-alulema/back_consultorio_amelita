namespace ClinicaAmelita.Api.Entities;

public enum EstadoCita
{
    Agendada = 1,
    Confirmada = 2,
    EnEspera = 3,
    Atendida = 4,
    Anulada = 5,
    NoAsistio = 6
}

public class Cita : BaseEntity
{
    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; } = null!;
    public int? MedicoUsuarioId { get; set; }
    public Usuario? MedicoUsuario { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime? FechaHoraFin { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public EstadoCita Estado { get; set; } = EstadoCita.Agendada;
    public int? ConsultaId { get; set; }
    public Consulta? Consulta { get; set; }
}
