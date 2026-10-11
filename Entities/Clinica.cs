namespace ClinicaAmelita.Api.Entities;

public class Paciente : BaseEntity
{
    public string Identificacion { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateOnly? FechaNacimiento { get; set; }
    public string? Sexo { get; set; }
    public string? TipoSangre { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? ContactoEmergencia { get; set; }
    public string? TelefonoEmergencia { get; set; }
    public ICollection<HistoriaClinica> HistoriasClinicas { get; set; } = new List<HistoriaClinica>();
    public ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
}

public class HistoriaClinica : BaseEntity
{
    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; } = null!;
    public string? Alergias { get; set; }
    public string? AntecedentesPersonales { get; set; }
    public string? AntecedentesFamiliares { get; set; }
    public string? Cirugias { get; set; }
    public string? MedicacionHabitual { get; set; }
    public string? Observaciones { get; set; }
}

public class Consulta : BaseEntity
{
    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; } = null!;
    public int MedicoUsuarioId { get; set; }
    public Usuario MedicoUsuario { get; set; } = null!;
    public DateTime FechaConsulta { get; set; } = DateTime.UtcNow;
    public string MotivoConsulta { get; set; } = string.Empty;
    public string? EnfermedadActual { get; set; }
    public decimal? PesoKg { get; set; }
    public decimal? TallaCm { get; set; }
    public decimal? Temperatura { get; set; }
    public string? PresionArterial { get; set; }
    public int? FrecuenciaCardiaca { get; set; }
    public int? SaturacionOxigeno { get; set; }
    public string? ExamenFisico { get; set; }
    public string? Diagnostico { get; set; }
    public string? Tratamiento { get; set; }
    public string? Indicaciones { get; set; }
    public string? Observaciones { get; set; }
    public decimal ValorConsulta { get; set; }
    public string FormaPago { get; set; } = "Efectivo";
    public ICollection<Receta> Recetas { get; set; } = new List<Receta>();
}

public class Receta : BaseEntity
{
    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; } = null!;
    public int? ConsultaId { get; set; }
    public Consulta? Consulta { get; set; }
    public int MedicoUsuarioId { get; set; }
    public Usuario MedicoUsuario { get; set; } = null!;
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public string? IndicacionesGenerales { get; set; }
    public ICollection<RecetaDetalle> Detalles { get; set; } = new List<RecetaDetalle>();
}

public class RecetaDetalle : BaseEntity
{
    public int RecetaId { get; set; }
    public Receta Receta { get; set; } = null!;
    public int? ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public string Medicamento { get; set; } = string.Empty;
    public string? Presentacion { get; set; }
    public string Dosis { get; set; } = string.Empty;
    public string Frecuencia { get; set; } = string.Empty;
    public string Duracion { get; set; } = string.Empty;
    public decimal? Cantidad { get; set; }
    public string? Indicaciones { get; set; }
}
