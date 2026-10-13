using ClosedXML.Excel;
using ClinicaAmelita.Api.Data;
using ClinicaAmelita.Api.Entities;
using ClinicaAmelita.Api.Helpers;
using ClinicaAmelita.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAmelita.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly InventarioService _inventarioService;

    public ProductosController(AppDbContext db, InventarioService inventarioService)
    {
        _db = db;
        _inventarioService = inventarioService;
    }

    public record ProductoRequest(
        string Codigo,
        string Nombre,
        string? PrincipioActivo,
        string? Laboratorio,
        string? Presentacion,
        string? Concentracion,
        string? UnidadMedida,
        decimal? UnidadesPorCaja,
        decimal? PrecioCaja,
        decimal? PrecioCajaDescuento,
        decimal PrecioCompra,
        decimal PrecioVenta,
        decimal StockMinimo,
        string? NumeroLote,
        DateOnly? FechaCaducidad
    );

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? buscar,
        [FromQuery] string? estado)
    {
        var productos = await _db.Productos
            .AsNoTracking()
            .Where(x =>
                x.EsActivo
                && (
                    string.IsNullOrWhiteSpace(buscar)
                    || x.Codigo.Contains(buscar)
                    || x.Nombre.ToLower().Contains(buscar.ToLower())
                )
            )
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Nombre,
                x.PrincipioActivo,
                x.Laboratorio,
                x.Presentacion,
                x.Concentracion,
                x.UnidadMedida,
                x.UnidadesPorCaja,
                x.PrecioCaja,
                x.PrecioCajaDescuento,
                x.PrecioCompra,
                x.PrecioVenta,
                x.StockMinimo,
                Stock = x.Lotes
                    .Where(l => l.EsActivo)
                    .Sum(l => l.StockActual),
                ProximaCaducidad = x.Lotes
                    .Where(l => l.EsActivo && l.FechaCaducidad != null)
                    .OrderBy(l => l.FechaCaducidad)
                    .Select(l => l.FechaCaducidad)
                    .FirstOrDefault(),
                NumeroLote = x.Lotes
                    .Where(l => l.EsActivo && l.FechaCaducidad != null)
                    .OrderBy(l => l.FechaCaducidad)
                    .Select(l => l.NumeroLote)
                    .FirstOrDefault()
            })
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        var resultado = productos.Select(x => new
        {
            x.Id,
            x.Codigo,
            x.Nombre,
            x.PrincipioActivo,
            x.Laboratorio,
            x.Presentacion,
            x.Concentracion,
            x.UnidadMedida,
            x.UnidadesPorCaja,
            x.PrecioCaja,
            x.PrecioCajaDescuento,
            x.PrecioCompra,
            x.PrecioVenta,
            x.StockMinimo,
            x.Stock,
            x.ProximaCaducidad,
            x.NumeroLote,
            Estado = x.Stock <= 0
                ? "AGOTADO"
                : x.Stock <= x.StockMinimo
                    ? "STOCK_BAJO"
                    : "OK"
        });

        if (!string.IsNullOrWhiteSpace(estado))
        {
            resultado = resultado.Where(x => x.Estado == estado.ToUpper());
        }

        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductoRequest request)
    {
        var existe = await _db.Productos.AnyAsync(x => x.Codigo == request.Codigo);

        if (existe)
        {
            return BadRequest("El código ya existe.");
        }

        var producto = new Producto
        {
            Codigo = request.Codigo,
            Nombre = request.Nombre,
            PrincipioActivo = request.PrincipioActivo,
            Laboratorio = request.Laboratorio,
            Presentacion = request.Presentacion,
            Concentracion = request.Concentracion,
            UnidadMedida = request.UnidadMedida,
            UnidadesPorCaja = request.UnidadesPorCaja,
            PrecioCaja = request.PrecioCaja,
            PrecioCajaDescuento = request.PrecioCajaDescuento,
            PrecioCompra = request.PrecioCompra,
            PrecioVenta = request.PrecioVenta,
            StockMinimo = request.StockMinimo,
            UsuarioCreacionId = User.GetUserId()
        };

        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();

        if (request.FechaCaducidad.HasValue || !string.IsNullOrWhiteSpace(request.NumeroLote))
        {
            _db.LotesProductos.Add(new LoteProducto
            {
                ProductoId = producto.Id,
                NumeroLote = string.IsNullOrWhiteSpace(request.NumeroLote)
                    ? "SIN-LOTE"
                    : request.NumeroLote.Trim(),
                FechaCaducidad = request.FechaCaducidad,
                StockActual = 0,
                PrecioCompra = request.PrecioCompra,
                UsuarioCreacionId = User.GetUserId()
            });

            await _db.SaveChangesAsync();
        }

        return Ok(producto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductoRequest request)
    {
        var producto = await _db.Productos.FindAsync(id);

        if (producto is null)
        {
            return NotFound();
        }

        producto.Codigo = request.Codigo;
        producto.Nombre = request.Nombre;
        producto.PrincipioActivo = request.PrincipioActivo;
        producto.Laboratorio = request.Laboratorio;
        producto.Presentacion = request.Presentacion;
        producto.Concentracion = request.Concentracion;
        producto.UnidadMedida = request.UnidadMedida;
        producto.UnidadesPorCaja = request.UnidadesPorCaja;
        producto.PrecioCaja = request.PrecioCaja;
        producto.PrecioCajaDescuento = request.PrecioCajaDescuento;
        producto.PrecioCompra = request.PrecioCompra;
        producto.PrecioVenta = request.PrecioVenta;
        producto.StockMinimo = request.StockMinimo;
        producto.UsuarioActualizacionId = User.GetUserId();

        if (request.FechaCaducidad.HasValue || !string.IsNullOrWhiteSpace(request.NumeroLote))
        {
            var lote = await _db.LotesProductos
                .Where(x => x.ProductoId == producto.Id && x.EsActivo)
                .OrderBy(x => x.FechaCaducidad)
                .FirstOrDefaultAsync();

            if (lote is null)
            {
                lote = new LoteProducto
                {
                    ProductoId = producto.Id,
                    NumeroLote = string.IsNullOrWhiteSpace(request.NumeroLote)
                        ? "SIN-LOTE"
                        : request.NumeroLote.Trim(),
                    FechaCaducidad = request.FechaCaducidad,
                    StockActual = 0,
                    PrecioCompra = request.PrecioCompra,
                    UsuarioCreacionId = User.GetUserId()
                };

                _db.LotesProductos.Add(lote);
            }
            else
            {
                lote.NumeroLote = string.IsNullOrWhiteSpace(request.NumeroLote)
                    ? lote.NumeroLote
                    : request.NumeroLote.Trim();
                lote.FechaCaducidad = request.FechaCaducidad;
                lote.UsuarioActualizacionId = User.GetUserId();
            }
        }

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id:int}/baja")]
    public async Task<IActionResult> Baja(int id)
    {
        var producto = await _db.Productos.FindAsync(id);

        if (producto is null)
        {
            return NotFound();
        }

        producto.EsActivo = false;
        producto.UsuarioActualizacionId = User.GetUserId();

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("importar-excel")]
    public async Task<IActionResult> ImportarExcel(IFormFile archivo)
    {
        if (archivo.Length == 0)
        {
            return BadRequest("Archivo vacío.");
        }

        var creados = 0;
        var actualizados = 0;
        var filasConError = new List<object>();

        using var stream = archivo.OpenReadStream();
        using var workbook = new XLWorkbook(stream);

        var worksheet = workbook.Worksheet("INVENTARIO");
        var headerRow = worksheet.RowsUsed()
            .FirstOrDefault(row => row.CellsUsed().Any(cell =>
                cell.GetString()
                    .Trim()
                    .Equals("CÓDIGO", StringComparison.OrdinalIgnoreCase)
            ))
            ?.RowNumber() ?? 4;

        var ultimaFila = worksheet.LastRowUsed()?.RowNumber() ?? headerRow;

        for (var fila = headerRow + 1; fila <= ultimaFila; fila++)
        {
            try
            {
                var codigo = worksheet.Cell(fila, 2).GetString().Trim();
                var nombre = worksheet.Cell(fila, 3).GetString().Trim();

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(codigo))
                {
                    codigo = $"IMP-{fila:00000}";
                }

                var laboratorio = worksheet.Cell(fila, 4).GetString().Trim();
                var lote = worksheet.Cell(fila, 5).GetString().Trim();
                var caducidad = ObtenerFecha(worksheet.Cell(fila, 6).GetString());
                var unidadesPorCaja = ObtenerDecimalNullable(worksheet.Cell(fila, 8));
                var precioCaja = ObtenerDecimalNullable(worksheet.Cell(fila, 10));
                var precioCajaDescuento = ObtenerDecimalNullable(worksheet.Cell(fila, 11));
                var precioVenta = ObtenerDecimal(worksheet.Cell(fila, 12));
                var stockActual = ObtenerDecimal(worksheet.Cell(fila, 15));
                var precioCompra = CalcularPrecioCompra(
                    unidadesPorCaja,
                    precioCaja,
                    precioCajaDescuento
                );

                var producto = await _db.Productos
                    .FirstOrDefaultAsync(x => x.Codigo == codigo);

                if (producto is null)
                {
                    producto = new Producto
                    {
                        Codigo = codigo,
                        Nombre = nombre,
                        Laboratorio = laboratorio,
                        UnidadesPorCaja = unidadesPorCaja,
                        PrecioCaja = precioCaja,
                        PrecioCajaDescuento = precioCajaDescuento,
                        PrecioCompra = precioCompra,
                        PrecioVenta = precioVenta,
                        StockMinimo = 5,
                        UsuarioCreacionId = User.GetUserId()
                    };

                    _db.Productos.Add(producto);
                    await _db.SaveChangesAsync();
                    creados++;
                }
                else
                {
                    producto.Nombre = nombre;
                    producto.Laboratorio = laboratorio;
                    producto.UnidadesPorCaja = unidadesPorCaja;
                    producto.PrecioCaja = precioCaja;
                    producto.PrecioCajaDescuento = precioCajaDescuento;
                    producto.PrecioCompra = precioCompra;
                    producto.PrecioVenta = precioVenta;
                    producto.UsuarioActualizacionId = User.GetUserId();

                    await _db.SaveChangesAsync();
                    actualizados++;
                }

                var numeroLote = string.IsNullOrWhiteSpace(lote)
                    ? "SIN-LOTE"
                    : lote;

                var loteExistente = await _db.LotesProductos
                    .FirstOrDefaultAsync(x =>
                        x.ProductoId == producto.Id
                        && x.NumeroLote == numeroLote
                    );

                if (loteExistente is null && stockActual > 0)
                {
                    await _inventarioService.RegistrarEntrada(
                        producto.Id,
                        numeroLote,
                        caducidad,
                        stockActual,
                        TipoMovimientoInventario.CargaInicial,
                        User.GetUserId(),
                        $"Importación {archivo.FileName}",
                        precioCompra
                    );
                }
            }
            catch (Exception ex)
            {
                filasConError.Add(new
                {
                    fila,
                    error = ex.Message
                });
            }
        }

        return Ok(new
        {
            creados,
            actualizados,
            errores = filasConError.Count,
            filasConError
        });
    }

    private static DateOnly? ObtenerFecha(string valor)
    {
        return DateTime.TryParse(valor.Trim(), out var fecha)
            ? DateOnly.FromDateTime(fecha)
            : null;
    }

    private static decimal? ObtenerDecimalNullable(IXLCell celda)
    {
        return celda.TryGetValue<decimal>(out var valor)
            ? valor
            : null;
    }

    private static decimal ObtenerDecimal(IXLCell celda)
    {
        return celda.TryGetValue<decimal>(out var valor)
            ? valor
            : 0;
    }

    private static decimal CalcularPrecioCompra(
        decimal? unidadesPorCaja,
        decimal? precioCaja,
        decimal? precioCajaDescuento)
    {
        if (unidadesPorCaja.GetValueOrDefault() <= 0)
        {
            return 0;
        }

        return (precioCajaDescuento ?? precioCaja ?? 0) / unidadesPorCaja!.Value;
    }
}
