namespace ClinicaAmelita.Api.Entities;

public enum EstadoCaja
{
    Abierta = 1,
    Cerrada = 2
}

public enum TipoMovimientoCaja
{
    Ingreso = 1,
    Egreso = 2,
    Retiro = 3
}

public enum TipoConteoCaja
{
    Apertura = 1,
    Cierre = 2
}

public class Caja : BaseEntity
{
    public DateOnly Fecha { get; set; }
    public DateTime FechaApertura { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; set; }
    public decimal MontoApertura { get; set; }
    public decimal? MontoEsperado { get; set; }
    public decimal? MontoContado { get; set; }
    public decimal? Diferencia { get; set; }
    public EstadoCaja Estado { get; set; } = EstadoCaja.Abierta;
    public string? ObservacionesApertura { get; set; }
    public string? ObservacionesCierre { get; set; }
    public int UsuarioAperturaId { get; set; }
    public Usuario UsuarioApertura { get; set; } = null!;
    public int? UsuarioCierreId { get; set; }
    public Usuario? UsuarioCierre { get; set; }
    public ICollection<MovimientoCaja> Movimientos { get; set; } = new List<MovimientoCaja>();
    public ICollection<ConteoCaja> Conteos { get; set; } = new List<ConteoCaja>();
}

public class MovimientoCaja : BaseEntity
{
    public int CajaId { get; set; }
    public Caja Caja { get; set; } = null!;
    public TipoMovimientoCaja Tipo { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;
    public string? Referencia { get; set; }
}

public class ConteoCaja : BaseEntity
{
    public int CajaId { get; set; }
    public Caja Caja { get; set; } = null!;
    public TipoConteoCaja Tipo { get; set; }
    public decimal Denominacion { get; set; }
    public int Cantidad { get; set; }
    public decimal Total => Denominacion * Cantidad;
}
