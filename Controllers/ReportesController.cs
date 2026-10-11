using ClinicaAmelita.Api.Data;
using ClinicaAmelita.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAmelita.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReportesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var inicio = DateTime.UtcNow.Date;
        var fin = inicio.AddDays(1);

        var consultas = await _db.Consultas.CountAsync(x =>
            x.EsActivo
            && x.FechaConsulta >= inicio
            && x.FechaConsulta < fin
        );

        var valorConsultas = await _db.Consultas
            .Where(x =>
                x.EsActivo
                && x.FechaConsulta >= inicio
                && x.FechaConsulta < fin
            )
            .SumAsync(x => (decimal?)x.ValorConsulta) ?? 0;

        var ventas = await _db.Ventas.CountAsync(x =>
            x.EsActivo
            && x.FechaVenta >= inicio
            && x.FechaVenta < fin
        );

        var totalVentas = await _db.Ventas
            .Where(x =>
                x.EsActivo
                && x.FechaVenta >= inicio
                && x.FechaVenta < fin
            )
            .SumAsync(x => (decimal?)x.Total) ?? 0;

        var stocks = await _db.Productos
            .Where(x => x.EsActivo)
            .Select(x => new
            {
                x.StockMinimo,
                Stock = x.Lotes
                    .Where(l => l.EsActivo)
                    .Sum(l => l.StockActual)
            })
            .ToListAsync();

        var pedidos = await _db.Pedidos.CountAsync(x =>
            x.EsActivo
            && (
                x.Estado == EstadoPedido.Solicitado
                || x.Estado == EstadoPedido.Parcial
            )
        );

        return Ok(new
        {
            consultasHoy = consultas,
            valorConsultasHoy = valorConsultas,
            ventasHoy = ventas,
            totalVentasHoy = totalVentas,
            stockBajo = stocks.Count(x => x.Stock > 0 && x.Stock <= x.StockMinimo),
            agotados = stocks.Count(x => x.Stock <= 0),
            pedidosPendientes = pedidos
        });
    }

    [HttpGet("ventas-diarias")]
    public async Task<IActionResult> VentasDiarias(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        var ventas = await _db.Ventas
            .AsNoTracking()
            .Where(x =>
                x.EsActivo
                && x.FechaVenta >= desde.ToUniversalTime()
                && x.FechaVenta < hasta.ToUniversalTime().AddDays(1)
            )
            .GroupBy(x => x.FechaVenta.Date)
            .Select(g => new
            {
                Fecha = g.Key,
                Cantidad = g.Count(),
                Total = g.Sum(x => x.Total)
            })
            .OrderBy(x => x.Fecha)
            .ToListAsync();

        return Ok(ventas);
    }

    [HttpGet("consultas-diarias")]
    public async Task<IActionResult> ConsultasDiarias(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        var consultas = await _db.Consultas
            .AsNoTracking()
            .Where(x =>
                x.EsActivo
                && x.FechaConsulta >= desde.ToUniversalTime()
                && x.FechaConsulta < hasta.ToUniversalTime().AddDays(1)
            )
            .GroupBy(x => x.FechaConsulta.Date)
            .Select(g => new
            {
                Fecha = g.Key,
                Cantidad = g.Count(),
                Total = g.Sum(x => x.ValorConsulta)
            })
            .OrderBy(x => x.Fecha)
            .ToListAsync();

        return Ok(consultas);
    }

    [HttpGet("productos-mas-vendidos")]
    public async Task<IActionResult> ProductosMasVendidos(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        var productos = await _db.VentasDetalles
            .AsNoTracking()
            .Include(x => x.Venta)
            .Include(x => x.Producto)
            .Where(x =>
                x.Venta.EsActivo
                && x.Venta.FechaVenta >= desde.ToUniversalTime()
                && x.Venta.FechaVenta < hasta.ToUniversalTime().AddDays(1)
            )
            .GroupBy(x => new
            {
                x.ProductoId,
                x.Producto.Nombre
            })
            .Select(g => new
            {
                g.Key.ProductoId,
                g.Key.Nombre,
                Cantidad = g.Sum(x => x.Cantidad),
                Total = g.Sum(x => x.Total)
            })
            .OrderByDescending(x => x.Cantidad)
            .Take(20)
            .ToListAsync();

        return Ok(productos);
    }
}
