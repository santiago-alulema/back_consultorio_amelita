using ClinicaAmelita.Api.Data;
using ClinicaAmelita.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAmelita.Api.Services;

public class InventarioService
{
    private readonly AppDbContext _db;

    public InventarioService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<LoteProducto> RegistrarEntrada(
        int productoId,
        string lote,
        DateOnly? caducidad,
        decimal cantidad,
        TipoMovimientoInventario tipo,
        int usuarioId,
        string? referencia = null,
        decimal? precioCompra = null)
    {
        var entidad = await _db.LotesProductos
            .FirstOrDefaultAsync(x =>
                x.ProductoId == productoId
                && x.NumeroLote == lote
                && x.EsActivo
            );

        if (entidad is null)
        {
            entidad = new LoteProducto
            {
                ProductoId = productoId,
                NumeroLote = lote,
                FechaCaducidad = caducidad,
                StockActual = 0,
                PrecioCompra = precioCompra,
                UsuarioCreacionId = usuarioId
            };

            _db.LotesProductos.Add(entidad);
            await _db.SaveChangesAsync();
        }

        var stockAnterior = entidad.StockActual;
        entidad.StockActual += cantidad;

        _db.MovimientosInventario.Add(new MovimientoInventario
        {
            ProductoId = productoId,
            LoteProductoId = entidad.Id,
            Tipo = tipo,
            Cantidad = cantidad,
            StockAnterior = stockAnterior,
            StockPosterior = entidad.StockActual,
            Referencia = referencia,
            UsuarioCreacionId = usuarioId
        });

        await _db.SaveChangesAsync();

        return entidad;
    }

    public async Task RegistrarSalida(
        int productoId,
        decimal cantidad,
        TipoMovimientoInventario tipo,
        int usuarioId,
        string? referencia = null)
    {
        var restante = cantidad;

        var lotes = await _db.LotesProductos
            .Where(x =>
                x.ProductoId == productoId
                && x.EsActivo
                && x.StockActual > 0
            )
            .OrderBy(x => x.FechaCaducidad == null)
            .ThenBy(x => x.FechaCaducidad)
            .ThenBy(x => x.Id)
            .ToListAsync();

        if (lotes.Sum(x => x.StockActual) < cantidad)
        {
            throw new InvalidOperationException("Stock insuficiente para el producto.");
        }

        foreach (var lote in lotes)
        {
            if (restante <= 0)
            {
                break;
            }

            var salida = Math.Min(lote.StockActual, restante);
            var stockAnterior = lote.StockActual;

            lote.StockActual -= salida;
            restante -= salida;

            _db.MovimientosInventario.Add(new MovimientoInventario
            {
                ProductoId = productoId,
                LoteProductoId = lote.Id,
                Tipo = tipo,
                Cantidad = salida,
                StockAnterior = stockAnterior,
                StockPosterior = lote.StockActual,
                Referencia = referencia,
                UsuarioCreacionId = usuarioId
            });
        }

        await _db.SaveChangesAsync();
    }
}
