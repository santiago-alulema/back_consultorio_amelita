namespace ClinicaAmelita.Api.Entities;

public class Producto : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? PrincipioActivo { get; set; }
    public string? Laboratorio { get; set; }
    public string? Presentacion { get; set; }
    public string? Concentracion { get; set; }
    public string? UnidadMedida { get; set; }
    public decimal? UnidadesPorCaja { get; set; }
    public decimal? PrecioCaja { get; set; }
    public decimal? PrecioCajaDescuento { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal StockMinimo { get; set; }
    public ICollection<LoteProducto> Lotes { get; set; } = new List<LoteProducto>();
}

public class LoteProducto : BaseEntity
{
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public string NumeroLote { get; set; } = string.Empty;
    public DateOnly? FechaCaducidad { get; set; }
    public decimal StockActual { get; set; }
    public decimal? PrecioCompra { get; set; }
}

public enum TipoMovimientoInventario
{
    EntradaCompra = 1,
    EntradaPedido = 2,
    SalidaVenta = 3,
    SalidaFacturaSri = 4,
    AjustePositivo = 5,
    AjusteNegativo = 6,
    Baja = 7,
    Devolucion = 8,
    CargaInicial = 9
}

public class MovimientoInventario : BaseEntity
{
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int? LoteProductoId { get; set; }
    public LoteProducto? LoteProducto { get; set; }
    public TipoMovimientoInventario Tipo { get; set; }
    public decimal Cantidad { get; set; }
    public decimal StockAnterior { get; set; }
    public decimal StockPosterior { get; set; }
    public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;
    public string? Referencia { get; set; }
    public string? Motivo { get; set; }
}

public class Proveedor : BaseEntity
{
    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
}

public enum EstadoPedido { Borrador = 1, Solicitado = 2, Parcial = 3, Recibido = 4, Cancelado = 5 }

public class Pedido : BaseEntity
{
    public int? ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }
    public DateTime FechaPedido { get; set; } = DateTime.UtcNow;
    public EstadoPedido Estado { get; set; } = EstadoPedido.Borrador;
    public string? Observaciones { get; set; }
    public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
    public ICollection<RecepcionPedido> Recepciones { get; set; } = new List<RecepcionPedido>();
}

public class PedidoDetalle : BaseEntity
{
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public decimal CantidadSolicitada { get; set; }
    public decimal CantidadRecibida { get; set; }
    public decimal? PrecioUnitario { get; set; }
}

public class RecepcionPedido : BaseEntity
{
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public DateTime FechaRecepcion { get; set; } = DateTime.UtcNow;
    public string? Observaciones { get; set; }
    public ICollection<RecepcionPedidoDetalle> Detalles { get; set; } = new List<RecepcionPedidoDetalle>();
}

public class RecepcionPedidoDetalle : BaseEntity
{
    public int RecepcionPedidoId { get; set; }
    public RecepcionPedido RecepcionPedido { get; set; } = null!;
    public int PedidoDetalleId { get; set; }
    public int ProductoId { get; set; }
    public decimal Cantidad { get; set; }
    public string NumeroLote { get; set; } = string.Empty;
    public DateOnly? FechaCaducidad { get; set; }
    public decimal? PrecioCompra { get; set; }
}

public class Venta : BaseEntity
{
    public DateTime FechaVenta { get; set; } = DateTime.UtcNow;
    public string? IdentificacionCliente { get; set; }
    public string? NombreCliente { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public string? FormaPago { get; set; }
    public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
}

public class VentaDetalle : BaseEntity
{
    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
}

public class DocumentoSri : BaseEntity
{
    public string ClaveAcceso { get; set; } = string.Empty;
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public string RucEmisor { get; set; } = string.Empty;
    public string RazonSocialEmisor { get; set; } = string.Empty;
    public string? IdentificacionComprador { get; set; }
    public decimal Total { get; set; }
    public string XmlPath { get; set; } = string.Empty;
    public string? RidePdfPath { get; set; }
    public bool InventarioProcesado { get; set; }
    public ICollection<DocumentoSriDetalle> Detalles { get; set; } = new List<DocumentoSriDetalle>();
}

public class DocumentoSriDetalle : BaseEntity
{
    public int DocumentoSriId { get; set; }
    public DocumentoSri DocumentoSri { get; set; } = null!;
    public string CodigoPrincipal { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PrecioTotalSinImpuesto { get; set; }
    public int? ProductoId { get; set; }
}
