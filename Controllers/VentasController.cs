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
[Route("api/ventas")]
public class VentasController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly InventarioService _inventarioService;

    public VentasController(AppDbContext db, InventarioService inventarioService)
    {
        _db = db;
        _inventarioService = inventarioService;
    }

    public record DetalleRequest(
        int ProductoId,
        decimal Cantidad,
        decimal PrecioUnitario
    );

    public record VentaRequest(
        string? IdentificacionCliente,
        string? NombreCliente,
        string? FormaPago,
        List<DetalleRequest> Detalles
    );

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        var ventas = await _db.Ventas
            .AsNoTracking()
            .Include(x => x.Detalles)
            .ThenInclude(x => x.Producto)
            .Where(x =>
                x.EsActivo
                && (!desde.HasValue || x.FechaVenta >= desde.Value.ToUniversalTime())
                && (!hasta.HasValue || x.FechaVenta < hasta.Value.ToUniversalTime().AddDays(1))
            )
            .OrderByDescending(x => x.FechaVenta)
            .Select(x => new
            {
                x.Id,
                x.FechaVenta,
                x.IdentificacionCliente,
                x.NombreCliente,
                x.FormaPago,
                x.Total,
                Detalles = x.Detalles.Select(d => new
                {
                    d.ProductoId,
                    d.Producto.Nombre,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Total
                })
            })
            .ToListAsync();

        return Ok(ventas);
    }

    [HttpPost]
    public async Task<IActionResult> Create(VentaRequest request)
    {
        if (request.Detalles.Count == 0)
        {
            return BadRequest("Debe ingresar productos.");
        }

        await using var transaccion = await _db.Database.BeginTransactionAsync();

        try
        {
            var usuarioId = User.GetUserId();
            var venta = new Venta
            {
                IdentificacionCliente = request.IdentificacionCliente,
                NombreCliente = request.NombreCliente,
                FormaPago = request.FormaPago,
                UsuarioCreacionId = usuarioId
            };

            foreach (var detalle in request.Detalles)
            {
                var total = detalle.Cantidad * detalle.PrecioUnitario;

                venta.Detalles.Add(new VentaDetalle
                {
                    ProductoId = detalle.ProductoId,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Total = total,
                    UsuarioCreacionId = usuarioId
                });

                venta.Total += total;
            }

            _db.Ventas.Add(venta);
            await _db.SaveChangesAsync();

            foreach (var detalle in request.Detalles)
            {
                await _inventarioService.RegistrarSalida(
                    detalle.ProductoId,
                    detalle.Cantidad,
                    TipoMovimientoInventario.SalidaVenta,
                    usuarioId,
                    $"VENTA-{venta.Id}"
                );
            }

            await transaccion.CommitAsync();

            return Ok(new
            {
                venta.Id,
                venta.Total
            });
        }
        catch (Exception ex)
        {
            await transaccion.RollbackAsync();
            return BadRequest(ex.Message);
        }
    }
}
