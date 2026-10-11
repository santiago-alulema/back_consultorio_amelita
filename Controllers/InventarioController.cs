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
[Route("api/inventario")]
public class InventarioController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly InventarioService _inventarioService;

    public InventarioController(AppDbContext db, InventarioService inventarioService)
    {
        _db = db;
        _inventarioService = inventarioService;
    }

    public record AjusteRequest(
        int ProductoId,
        string NumeroLote,
        DateOnly? FechaCaducidad,
        decimal Cantidad,
        bool EsEntrada,
        string? Motivo
    );

    [HttpGet("stock")]
    public async Task<IActionResult> Stock()
    {
        var stock = await _db.Productos
            .AsNoTracking()
            .Where(x => x.EsActivo)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Nombre,
                x.StockMinimo,
                Stock = x.Lotes
                    .Where(l => l.EsActivo)
                    .Sum(l => l.StockActual),
                Lotes = x.Lotes
                    .Where(l => l.EsActivo)
                    .Select(l => new
                    {
                        l.Id,
                        l.NumeroLote,
                        l.FechaCaducidad,
                        l.StockActual
                    })
            })
            .ToListAsync();

        return Ok(stock);
    }

    [HttpGet("movimientos")]
    public async Task<IActionResult> Movimientos(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        var movimientos = await _db.MovimientosInventario
            .AsNoTracking()
            .Include(x => x.Producto)
            .Include(x => x.LoteProducto)
            .Where(x =>
                (!desde.HasValue || x.FechaMovimiento >= desde.Value.ToUniversalTime())
                && (!hasta.HasValue || x.FechaMovimiento < hasta.Value.ToUniversalTime().AddDays(1))
            )
            .OrderByDescending(x => x.FechaMovimiento)
            .Take(500)
            .Select(x => new
            {
                x.Id,
                x.FechaMovimiento,
                Producto = x.Producto.Nombre,
                Lote = x.LoteProducto != null
                    ? x.LoteProducto.NumeroLote
                    : null,
                x.Tipo,
                x.Cantidad,
                x.StockAnterior,
                x.StockPosterior,
                x.Referencia,
                x.Motivo
            })
            .ToListAsync();

        return Ok(movimientos);
    }

    [HttpPost("ajuste")]
    public async Task<IActionResult> Ajuste(AjusteRequest request)
    {
        if (request.Cantidad <= 0)
        {
            return BadRequest("Cantidad inválida.");
        }

        if (request.EsEntrada)
        {
            await _inventarioService.RegistrarEntrada(
                request.ProductoId,
                request.NumeroLote,
                request.FechaCaducidad,
                request.Cantidad,
                TipoMovimientoInventario.AjustePositivo,
                User.GetUserId(),
                null,
                null
            );
        }
        else
        {
            await _inventarioService.RegistrarSalida(
                request.ProductoId,
                request.Cantidad,
                TipoMovimientoInventario.AjusteNegativo,
                User.GetUserId(),
                request.Motivo
            );
        }

        return Ok();
    }
}
