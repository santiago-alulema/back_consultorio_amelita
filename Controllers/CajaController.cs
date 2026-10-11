using ClinicaAmelita.Api.Data;
using ClinicaAmelita.Api.Entities;
using ClinicaAmelita.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAmelita.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/caja")]
public class CajaController : ControllerBase
{
    private readonly AppDbContext _db;

    public CajaController(AppDbContext db)
    {
        _db = db;
    }

    public record ConteoRequest(decimal Denominacion, int Cantidad);
    public record AperturaRequest(List<ConteoRequest> Conteos, string? Observaciones);
    public record MovimientoRequest(TipoMovimientoCaja Tipo, string Concepto, decimal Monto, string? Referencia);
    public record CierreRequest(List<ConteoRequest> Conteos, string? Observaciones);

    [HttpGet("hoy")]
    public async Task<IActionResult> Hoy()
    {
        var fecha = EcuadorDateTime.Hoy();
        var caja = await _db.Cajas
            .AsNoTracking()
            .Include(x => x.UsuarioApertura)
            .Include(x => x.UsuarioCierre)
            .Include(x => x.Movimientos.Where(m => m.EsActivo))
            .Include(x => x.Conteos.Where(c => c.EsActivo))
            .FirstOrDefaultAsync(x => x.Fecha == fecha && x.EsActivo);

        if (caja is null)
        {
            return Ok(new
            {
                existe = false,
                fecha
            });
        }

        var resumen = await ObtenerResumenAsync(caja);

        return Ok(new
        {
            existe = true,
            caja = CrearRespuesta(caja, resumen)
        });
    }

    [HttpGet("historial")]
    public async Task<IActionResult> Historial([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
    {
        var query = _db.Cajas
            .AsNoTracking()
            .Include(x => x.UsuarioApertura)
            .Include(x => x.UsuarioCierre)
            .Where(x => x.EsActivo);

        if (desde.HasValue)
        {
            query = query.Where(x => x.Fecha >= desde.Value);
        }

        if (hasta.HasValue)
        {
            query = query.Where(x => x.Fecha <= hasta.Value);
        }

        var cajas = await query
            .OrderByDescending(x => x.Fecha)
            .Take(100)
            .ToListAsync();

        return Ok(cajas.Select(x => new
        {
            x.Id,
            x.Fecha,
            x.FechaApertura,
            x.FechaCierre,
            x.MontoApertura,
            x.MontoEsperado,
            x.MontoContado,
            x.Diferencia,
            Estado = x.Estado.ToString(),
            UsuarioApertura = $"{x.UsuarioApertura.Nombre} {x.UsuarioApertura.Apellido}",
            UsuarioCierre = x.UsuarioCierre == null
                ? null
                : $"{x.UsuarioCierre.Nombre} {x.UsuarioCierre.Apellido}"
        }));
    }

    [HttpPost("aperturar")]
    public async Task<IActionResult> Aperturar(AperturaRequest request)
    {
        var error = ValidarConteos(request.Conteos);

        if (error is not null)
        {
            return BadRequest(error);
        }

        var fecha = EcuadorDateTime.Hoy();
        var existente = await _db.Cajas.AnyAsync(x => x.Fecha == fecha && x.EsActivo);

        if (existente)
        {
            return BadRequest("Ya existe una caja registrada para el día de hoy.");
        }

        var usuarioId = User.GetUserId();
        var montoApertura = CalcularTotalConteo(request.Conteos);
        var caja = new Caja
        {
            Fecha = fecha,
            FechaApertura = DateTime.UtcNow,
            MontoApertura = montoApertura,
            Estado = EstadoCaja.Abierta,
            ObservacionesApertura = request.Observaciones,
            UsuarioAperturaId = usuarioId,
            UsuarioCreacionId = usuarioId,
            Conteos = CrearConteos(request.Conteos, TipoConteoCaja.Apertura, usuarioId)
        };

        _db.Cajas.Add(caja);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            caja.Id,
            caja.MontoApertura
        });
    }

    [HttpPost("movimientos")]
    public async Task<IActionResult> RegistrarMovimiento(MovimientoRequest request)
    {
        if (request.Monto <= 0)
        {
            return BadRequest("El monto debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(request.Concepto))
        {
            return BadRequest("El concepto es obligatorio.");
        }

        var caja = await ObtenerCajaAbiertaHoyAsync();

        if (caja is null)
        {
            return BadRequest("Debe aperturar la caja antes de registrar movimientos.");
        }

        var usuarioId = User.GetUserId();
        var movimiento = new MovimientoCaja
        {
            CajaId = caja.Id,
            Tipo = request.Tipo,
            Concepto = request.Concepto.Trim(),
            Monto = request.Monto,
            Referencia = request.Referencia,
            FechaMovimiento = DateTime.UtcNow,
            UsuarioCreacionId = usuarioId
        };

        _db.MovimientosCaja.Add(movimiento);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            movimiento.Id
        });
    }

    [HttpPost("cerrar")]
    public async Task<IActionResult> Cerrar(CierreRequest request)
    {
        var error = ValidarConteos(request.Conteos);

        if (error is not null)
        {
            return BadRequest(error);
        }

        var caja = await ObtenerCajaAbiertaHoyAsync();

        if (caja is null)
        {
            return BadRequest("No existe una caja abierta para el día de hoy.");
        }

        var resumen = await ObtenerResumenAsync(caja);
        var usuarioId = User.GetUserId();
        var montoContado = CalcularTotalConteo(request.Conteos);

        caja.MontoEsperado = resumen.EfectivoEsperado;
        caja.MontoContado = montoContado;
        caja.Diferencia = montoContado - resumen.EfectivoEsperado;
        caja.FechaCierre = DateTime.UtcNow;
        caja.Estado = EstadoCaja.Cerrada;
        caja.ObservacionesCierre = request.Observaciones;
        caja.UsuarioCierreId = usuarioId;
        caja.UsuarioActualizacionId = usuarioId;

        foreach (var conteo in CrearConteos(request.Conteos, TipoConteoCaja.Cierre, usuarioId))
        {
            conteo.CajaId = caja.Id;
            _db.ConteosCaja.Add(conteo);
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            caja.Id,
            caja.MontoEsperado,
            caja.MontoContado,
            caja.Diferencia
        });
    }

    private async Task<Caja?> ObtenerCajaAbiertaHoyAsync()
    {
        var fecha = EcuadorDateTime.Hoy();

        return await _db.Cajas
            .Include(x => x.Movimientos.Where(m => m.EsActivo))
            .Include(x => x.Conteos.Where(c => c.EsActivo))
            .FirstOrDefaultAsync(x =>
                x.Fecha == fecha
                && x.Estado == EstadoCaja.Abierta
                && x.EsActivo
            );
    }

    private async Task<ResumenCaja> ObtenerResumenAsync(Caja caja)
    {
        var (desdeUtc, hastaUtc) = EcuadorDateTime.RangoUtc(caja.Fecha);

        var ventas = await _db.Ventas
            .AsNoTracking()
            .Where(x => x.EsActivo && x.FechaVenta >= desdeUtc && x.FechaVenta < hastaUtc)
            .Select(x => new { x.Total, x.FormaPago })
            .ToListAsync();

        var consultas = await _db.Consultas
            .AsNoTracking()
            .Where(x => x.EsActivo && x.FechaConsulta >= desdeUtc && x.FechaConsulta < hastaUtc)
            .Select(x => new { Total = x.ValorConsulta, x.FormaPago })
            .ToListAsync();

        decimal SumarPorFormaPago(string formaPago)
        {
            var totalVentas = ventas
                .Where(x => EsFormaPago(x.FormaPago, formaPago))
                .Sum(x => x.Total);

            var totalConsultas = consultas
                .Where(x => EsFormaPago(x.FormaPago, formaPago))
                .Sum(x => x.Total);

            return totalVentas + totalConsultas;
        }

        var ventasEfectivo = SumarPorFormaPago("efectivo");
        var ventasTarjeta = SumarPorFormaPago("tarjeta");
        var ventasTransferencia = SumarPorFormaPago("transferencia");

        var otrosIngresosEfectivo = caja.Movimientos
            .Where(x => x.Tipo == TipoMovimientoCaja.Ingreso && x.EsActivo)
            .Sum(x => x.Monto);

        var gastosEfectivo = caja.Movimientos
            .Where(x => x.Tipo == TipoMovimientoCaja.Egreso && x.EsActivo)
            .Sum(x => x.Monto);

        var retirosEfectivo = caja.Movimientos
            .Where(x => x.Tipo == TipoMovimientoCaja.Retiro && x.EsActivo)
            .Sum(x => x.Monto);

        var efectivoEsperado = caja.MontoApertura
            + ventasEfectivo
            + otrosIngresosEfectivo
            - gastosEfectivo
            - retirosEfectivo;

        var ventasTotales = ventasEfectivo + ventasTarjeta + ventasTransferencia;
        var ingresosTotales = ventasTotales + otrosIngresosEfectivo;
        var gastosRetiros = gastosEfectivo + retirosEfectivo;

        return new ResumenCaja(
            ventasEfectivo,
            ventasTarjeta,
            ventasTransferencia,
            otrosIngresosEfectivo,
            gastosEfectivo,
            retirosEfectivo,
            ventasTotales,
            ingresosTotales,
            gastosRetiros,
            efectivoEsperado
        );
    }

    private static object CrearRespuesta(Caja caja, ResumenCaja resumen)
    {
        return new
        {
            caja.Id,
            caja.Fecha,
            caja.FechaApertura,
            caja.FechaCierre,
            caja.MontoApertura,
            caja.MontoEsperado,
            caja.MontoContado,
            caja.Diferencia,
            Estado = caja.Estado.ToString(),
            caja.ObservacionesApertura,
            caja.ObservacionesCierre,
            UsuarioApertura = $"{caja.UsuarioApertura.Nombre} {caja.UsuarioApertura.Apellido}",
            UsuarioCierre = caja.UsuarioCierre == null
                ? null
                : $"{caja.UsuarioCierre.Nombre} {caja.UsuarioCierre.Apellido}",
            resumen.VentasEfectivo,
            resumen.VentasTarjeta,
            resumen.VentasTransferencia,
            resumen.OtrosIngresosEfectivo,
            resumen.GastosEfectivo,
            resumen.RetirosEfectivo,
            resumen.VentasTotales,
            resumen.IngresosTotales,
            resumen.GastosRetiros,
            EfectivoEsperado = caja.Estado == EstadoCaja.Cerrada
                ? caja.MontoEsperado ?? resumen.EfectivoEsperado
                : resumen.EfectivoEsperado,
            ConteosApertura = caja.Conteos
                .Where(x => x.EsActivo && x.Tipo == TipoConteoCaja.Apertura)
                .OrderByDescending(x => x.Denominacion)
                .Select(x => new
                {
                    x.Denominacion,
                    x.Cantidad,
                    Total = x.Denominacion * x.Cantidad
                }),
            ConteosCierre = caja.Conteos
                .Where(x => x.EsActivo && x.Tipo == TipoConteoCaja.Cierre)
                .OrderByDescending(x => x.Denominacion)
                .Select(x => new
                {
                    x.Denominacion,
                    x.Cantidad,
                    Total = x.Denominacion * x.Cantidad
                }),
            Movimientos = caja.Movimientos
                .Where(x => x.EsActivo)
                .OrderByDescending(x => x.FechaMovimiento)
                .Select(x => new
                {
                    x.Id,
                    Tipo = x.Tipo.ToString(),
                    x.Concepto,
                    x.Monto,
                    x.FechaMovimiento,
                    x.Referencia
                })
        };
    }

    private static List<ConteoCaja> CrearConteos(
        IEnumerable<ConteoRequest> conteos,
        TipoConteoCaja tipo,
        int usuarioId)
    {
        return conteos
            .Where(x => x.Cantidad > 0)
            .Select(x => new ConteoCaja
            {
                Tipo = tipo,
                Denominacion = x.Denominacion,
                Cantidad = x.Cantidad,
                UsuarioCreacionId = usuarioId
            })
            .ToList();
    }

    private static decimal CalcularTotalConteo(IEnumerable<ConteoRequest> conteos)
    {
        return conteos.Sum(x => x.Denominacion * x.Cantidad);
    }

    private static string? ValidarConteos(IEnumerable<ConteoRequest>? conteos)
    {
        if (conteos is null)
        {
            return "Debe ingresar el conteo de efectivo.";
        }

        if (conteos.Any(x => x.Denominacion <= 0 || x.Cantidad < 0))
        {
            return "El conteo de efectivo contiene valores inválidos.";
        }

        return null;
    }

    private static bool EsFormaPago(string? valor, string esperado)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return esperado == "efectivo";
        }

        return valor.Trim().ToLowerInvariant().Contains(esperado);
    }

    private sealed record ResumenCaja(
        decimal VentasEfectivo,
        decimal VentasTarjeta,
        decimal VentasTransferencia,
        decimal OtrosIngresosEfectivo,
        decimal GastosEfectivo,
        decimal RetirosEfectivo,
        decimal VentasTotales,
        decimal IngresosTotales,
        decimal GastosRetiros,
        decimal EfectivoEsperado
    );
}
