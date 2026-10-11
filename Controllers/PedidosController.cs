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
[Route("api/pedidos")]
public class PedidosController(
    AppDbContext db,
    InventarioService inventario) : ControllerBase
{
    public record DetallePedido(
        int ProductoId,
        decimal CantidadSolicitada,
        decimal? PrecioUnitario
    );

    public record PedidoRequest(
        int? ProveedorId,
        string? Observaciones,
        List<DetallePedido> Detalles
    );

    public record DetalleRecepcion(
        int PedidoDetalleId,
        int ProductoId,
        decimal Cantidad,
        string NumeroLote,
        DateOnly? FechaCaducidad,
        decimal? PrecioCompra
    );

    public record RecepcionRequest(
        string? Observaciones,
        List<DetalleRecepcion> Detalles
    );

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var pedidos = await db.Pedidos
            .AsNoTracking()
            .Include(x => x.Proveedor)
            .Include(x => x.Detalles)
            .ThenInclude(x => x.Producto)
            .OrderByDescending(x => x.FechaPedido)
            .Select(x => new
            {
                x.Id,
                x.FechaPedido,
                x.Estado,
                Proveedor = x.Proveedor != null
                    ? x.Proveedor.RazonSocial
                    : null,
                x.Observaciones,
                Detalles = x.Detalles.Select(d => new
                {
                    d.Id,
                    d.ProductoId,
                    d.Producto.Nombre,
                    d.CantidadSolicitada,
                    d.CantidadRecibida,
                    d.PrecioUnitario
                })
            })
            .ToListAsync();

        return Ok(pedidos);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PedidoRequest request)
    {
        var userId = User.GetUserId();

        var pedido = new Pedido
        {
            ProveedorId = request.ProveedorId,
            Observaciones = request.Observaciones,
            Estado = EstadoPedido.Solicitado,
            UsuarioCreacionId = userId,
            Detalles = request.Detalles
                .Select(d => new PedidoDetalle
                {
                    ProductoId = d.ProductoId,
                    CantidadSolicitada = d.CantidadSolicitada,
                    PrecioUnitario = d.PrecioUnitario,
                    UsuarioCreacionId = userId
                })
                .ToList()
        };

        db.Pedidos.Add(pedido);

        await db.SaveChangesAsync();

        return Ok(new
        {
            pedido.Id
        });
    }

    [HttpPost("{id:int}/recibir")]
    public async Task<IActionResult> Recibir(int id, RecepcionRequest request)
    {
        var pedido = await db.Pedidos
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (pedido is null)
            return NotFound();

        await using var tx = await db.Database.BeginTransactionAsync();

        try
        {
            var userId = User.GetUserId();

            var recepcion = new RecepcionPedido
            {
                PedidoId = id,
                Observaciones = request.Observaciones,
                UsuarioCreacionId = userId
            };

            foreach (var detalle in request.Detalles)
            {
                var pedidoDetalle = pedido.Detalles
                    .FirstOrDefault(x => x.Id == detalle.PedidoDetalleId);

                if (pedidoDetalle is null)
                    throw new InvalidOperationException(
                        "Detalle de pedido no encontrado."
                    );

                pedidoDetalle.CantidadRecibida += detalle.Cantidad;

                recepcion.Detalles.Add(new RecepcionPedidoDetalle
                {
                    PedidoDetalleId = detalle.PedidoDetalleId,
                    ProductoId = detalle.ProductoId,
                    Cantidad = detalle.Cantidad,
                    NumeroLote = detalle.NumeroLote,
                    FechaCaducidad = detalle.FechaCaducidad,
                    PrecioCompra = detalle.PrecioCompra,
                    UsuarioCreacionId = userId
                });
            }

            db.RecepcionesPedidos.Add(recepcion);

            await db.SaveChangesAsync();

            foreach (var detalle in request.Detalles)
            {
                await inventario.RegistrarEntrada(
                    detalle.ProductoId,
                    detalle.NumeroLote,
                    detalle.FechaCaducidad,
                    detalle.Cantidad,
                    TipoMovimientoInventario.EntradaPedido,
                    userId,
                    $"PEDIDO-{id}",
                    detalle.PrecioCompra
                );
            }

            pedido.Estado = pedido.Detalles.All(
                x => x.CantidadRecibida >= x.CantidadSolicitada
            )
                ? EstadoPedido.Recibido
                : EstadoPedido.Parcial;

            await db.SaveChangesAsync();

            await tx.CommitAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();

            return BadRequest(ex.Message);
        }
    }
}
