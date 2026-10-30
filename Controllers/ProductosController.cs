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
        string? NumeroLote = null,
        DateOnly? FechaCaducidad = null,
        decimal StockInicial = 0
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
                    .Where(l => l.EsActivo && l.StockActual > 0)
                    .OrderBy(l => l.FechaCaducidad)
                    .Select(l => l.FechaCaducidad)
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

        if (request.StockInicial < 0 || (request.StockInicial > 0
            && (string.IsNullOrWhiteSpace(request.NumeroLote) || !request.FechaCaducidad.HasValue)))
        {
            return BadRequest("Indique lote y caducidad para registrar el stock inicial.");
        }

        await using var transaccion = await _db.Database.BeginTransactionAsync();
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

        if (!string.IsNullOrWhiteSpace(request.NumeroLote))
        {
            await _inventarioService.RegistrarEntrada(
                producto.Id, request.NumeroLote.Trim(), request.FechaCaducidad,
                request.StockInicial, TipoMovimientoInventario.CargaInicial,
                User.GetUserId(), "Registro de producto", request.PrecioCompra);
        }

        await transaccion.CommitAsync();
        return Ok(new { producto.Id });
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
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> ImportarExcel(IFormFile archivo)
    {
        if (archivo is null || archivo.Length == 0)
        {
            return BadRequest("Seleccione un archivo Excel.");
        }

        if (!Path.GetExtension(archivo.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("La plantilla debe estar en formato .xlsx.");
        }

        try
        {
            using var stream = archivo.OpenReadStream();
            using var workbook = new XLWorkbook(stream);

            var worksheet = workbook.Worksheets
                .FirstOrDefault(x => x.Name.Equals("INVENTARIO", StringComparison.OrdinalIgnoreCase));

            if (worksheet is null)
            {
                return BadRequest("El archivo debe contener una hoja llamada INVENTARIO.");
            }

            var headerRow = worksheet.RowsUsed()
                .FirstOrDefault(row => row.CellsUsed().Any(cell =>
                    cell.GetString()
                        .Trim()
                        .Equals("CÓDIGO", StringComparison.OrdinalIgnoreCase)
                ));

            if (headerRow is null)
            {
                return BadRequest("No se encontró la fila de encabezados de la plantilla.");
            }

            var creados = 0;
            var actualizados = 0;
            var filasConError = new List<object>();
            var numeroFilaEncabezado = headerRow.RowNumber();
            var ultimaFila = worksheet.LastRowUsed()?.RowNumber() ?? numeroFilaEncabezado;
            var usuarioId = User.GetUserId();

            for (var fila = numeroFilaEncabezado + 1; fila <= ultimaFila; fila++)
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
                    var numeroLote = worksheet.Cell(fila, 5).GetString().Trim();
                    var caducidad = ObtenerFecha(worksheet.Cell(fila, 6));
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
                            UsuarioCreacionId = usuarioId
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
                        producto.EsActivo = true;
                        producto.UsuarioActualizacionId = usuarioId;

                        await _db.SaveChangesAsync();
                        actualizados++;
                    }

                    numeroLote = string.IsNullOrWhiteSpace(numeroLote)
                        ? "SIN-LOTE"
                        : numeroLote;

                    await SincronizarLoteImportado(
                        producto,
                        numeroLote,
                        caducidad,
                        stockActual,
                        precioCompra,
                        usuarioId,
                        archivo.FileName
                    );
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
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = $"No se pudo leer el archivo Excel: {ex.Message}"
            });
        }
    }

    private async Task SincronizarLoteImportado(
        Producto producto,
        string numeroLote,
        DateOnly? caducidad,
        decimal stockActual,
        decimal precioCompra,
        int usuarioId,
        string nombreArchivo)
    {
        var lote = await _db.LotesProductos
            .FirstOrDefaultAsync(x =>
                x.ProductoId == producto.Id
                && x.NumeroLote == numeroLote
                && x.EsActivo
            );

        if (lote is null)
        {
            if (stockActual <= 0)
            {
                return;
            }

            await _inventarioService.RegistrarEntrada(
                producto.Id,
                numeroLote,
                caducidad,
                stockActual,
                TipoMovimientoInventario.CargaInicial,
                usuarioId,
                $"Importación {nombreArchivo}",
                precioCompra
            );

            return;
        }

        lote.FechaCaducidad = caducidad;
        lote.PrecioCompra = precioCompra;
        lote.UsuarioActualizacionId = usuarioId;

        var diferencia = stockActual - lote.StockActual;

        if (diferencia == 0)
        {
            await _db.SaveChangesAsync();
            return;
        }

        var stockAnterior = lote.StockActual;
        lote.StockActual = stockActual;

        _db.MovimientosInventario.Add(new MovimientoInventario
        {
            ProductoId = producto.Id,
            LoteProductoId = lote.Id,
            Tipo = diferencia > 0
                ? TipoMovimientoInventario.AjustePositivo
                : TipoMovimientoInventario.AjusteNegativo,
            Cantidad = Math.Abs(diferencia),
            StockAnterior = stockAnterior,
            StockPosterior = stockActual,
            Referencia = $"Importación {nombreArchivo}",
            Motivo = "Sincronización de stock desde plantilla Excel",
            UsuarioCreacionId = usuarioId
        });

        await _db.SaveChangesAsync();
    }

    private static DateOnly? ObtenerFecha(IXLCell celda)
    {
        if (celda.TryGetValue<DateTime>(out var fecha))
        {
            return DateOnly.FromDateTime(fecha);
        }

        return DateTime.TryParse(celda.GetString().Trim(), out fecha)
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
