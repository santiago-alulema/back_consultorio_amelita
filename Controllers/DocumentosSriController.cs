using System.Globalization;
using System.Xml.Linq;
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
[Route("api/documentos-sri")]
public class DocumentosSriController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly InventarioService _inventarioService;

    public DocumentosSriController(
        AppDbContext db,
        IWebHostEnvironment environment,
        InventarioService inventarioService)
    {
        _db = db;
        _environment = environment;
        _inventarioService = inventarioService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var documentos = await _db.DocumentosSri
            .AsNoTracking()
            .Include(x => x.Detalles)
            .OrderByDescending(x => x.FechaEmision)
            .Select(x => new
            {
                x.Id,
                x.ClaveAcceso,
                x.NumeroFactura,
                x.FechaEmision,
                x.RucEmisor,
                x.RazonSocialEmisor,
                x.IdentificacionComprador,
                x.Total,
                x.InventarioProcesado,
                x.XmlPath,
                x.RidePdfPath,
                Detalles = x.Detalles.Select(d => new
                {
                    d.Id,
                    d.CodigoPrincipal,
                    d.Descripcion,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.PrecioTotalSinImpuesto,
                    d.ProductoId
                })
            })
            .ToListAsync();

        return Ok(documentos);
    }

    [HttpPost("subir")]
    public async Task<IActionResult> Subir(IFormFile xml, IFormFile? ridePdf)
    {
        if (xml.Length == 0)
        {
            return BadRequest("Debe seleccionar el XML.");
        }

        using var reader = new StreamReader(xml.OpenReadStream());
        var contenido = await reader.ReadToEndAsync();

        XDocument documentoXml;

        try
        {
            documentoXml = XDocument.Parse(contenido);
        }
        catch
        {
            return BadRequest("XML inválido.");
        }

        var factura = documentoXml.Descendants("factura").FirstOrDefault();

        if (factura is null)
        {
            var comprobante = documentoXml
                .Descendants("comprobante")
                .FirstOrDefault()
                ?.Value;

            if (!string.IsNullOrWhiteSpace(comprobante))
            {
                factura = XDocument.Parse(comprobante).Root;
            }
        }

        if (factura is null)
        {
            return BadRequest("No se encontró la factura dentro del XML.");
        }

        string ObtenerValor(string nombre)
        {
            return factura.Descendants(nombre)
                .FirstOrDefault()
                ?.Value
                ?.Trim() ?? string.Empty;
        }

        decimal ObtenerDecimal(string nombre)
        {
            return decimal.TryParse(
                ObtenerValor(nombre),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var valor
            )
                ? valor
                : 0;
        }

        var claveAcceso = ObtenerValor("claveAcceso");

        if (string.IsNullOrWhiteSpace(claveAcceso))
        {
            claveAcceso = documentoXml
                .Descendants("claveAcceso")
                .FirstOrDefault()
                ?.Value
                ?.Trim() ?? Guid.NewGuid().ToString("N");
        }

        var yaExiste = await _db.DocumentosSri
            .AnyAsync(x => x.ClaveAcceso == claveAcceso);

        if (yaExiste)
        {
            return BadRequest("Esta factura ya fue cargada.");
        }

        var establecimiento = ObtenerValor("estab");
        var puntoEmision = ObtenerValor("ptoEmi");
        var secuencial = ObtenerValor("secuencial");
        var fechaEmision = ObtenerFechaEmision(ObtenerValor("fechaEmision"));
        var directorio = Path.Combine(
            _environment.ContentRootPath,
            "Uploads",
            "Facturas"
        );

        Directory.CreateDirectory(directorio);

        var nombreXml = $"{claveAcceso}.xml";
        await System.IO.File.WriteAllTextAsync(
            Path.Combine(directorio, nombreXml),
            contenido
        );

        string? nombrePdf = null;

        if (ridePdf is not null && ridePdf.Length > 0)
        {
            nombrePdf = $"{claveAcceso}.pdf";

            await using var streamPdf = System.IO.File.Create(
                Path.Combine(directorio, nombrePdf)
            );

            await ridePdf.CopyToAsync(streamPdf);
        }

        var usuarioId = User.GetUserId();
        var documentoSri = new DocumentoSri
        {
            ClaveAcceso = claveAcceso,
            NumeroFactura = $"{establecimiento}-{puntoEmision}-{secuencial}",
            FechaEmision = fechaEmision,
            RucEmisor = ObtenerValor("ruc"),
            RazonSocialEmisor = ObtenerValor("razonSocial"),
            IdentificacionComprador = ObtenerValor("identificacionComprador"),
            Total = ObtenerDecimal("importeTotal"),
            XmlPath = Path.Combine("Uploads", "Facturas", nombreXml),
            RidePdfPath = nombrePdf is null
                ? null
                : Path.Combine("Uploads", "Facturas", nombrePdf),
            UsuarioCreacionId = usuarioId
        };

        foreach (var detalleXml in factura.Descendants("detalle"))
        {
            string ValorDetalle(string nombre)
            {
                return detalleXml.Element(nombre)?.Value?.Trim() ?? string.Empty;
            }

            decimal DecimalDetalle(string nombre)
            {
                return decimal.TryParse(
                    ValorDetalle(nombre),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var valor
                )
                    ? valor
                    : 0;
            }

            var codigo = ValorDetalle("codigoPrincipal");
            var producto = await _db.Productos
                .FirstOrDefaultAsync(x => x.Codigo == codigo);

            documentoSri.Detalles.Add(new DocumentoSriDetalle
            {
                CodigoPrincipal = codigo,
                Descripcion = ValorDetalle("descripcion"),
                Cantidad = DecimalDetalle("cantidad"),
                PrecioUnitario = DecimalDetalle("precioUnitario"),
                PrecioTotalSinImpuesto = DecimalDetalle("precioTotalSinImpuesto"),
                ProductoId = producto?.Id,
                UsuarioCreacionId = usuarioId
            });
        }

        _db.DocumentosSri.Add(documentoSri);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            documentoSri.Id,
            documentoSri.NumeroFactura,
            detalles = documentoSri.Detalles.Count
        });
    }

    [HttpPost("{id:int}/procesar-stock")]
    public async Task<IActionResult> ProcesarStock(int id)
    {
        var documento = await _db.DocumentosSri
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (documento is null)
        {
            return NotFound();
        }

        if (documento.InventarioProcesado)
        {
            return BadRequest("El inventario de esta factura ya fue procesado.");
        }

        if (documento.Detalles.Any(x => !x.ProductoId.HasValue))
        {
            return BadRequest("Hay productos del XML sin relacionar al catálogo.");
        }

        await using var transaccion = await _db.Database.BeginTransactionAsync();

        try
        {
            foreach (var detalle in documento.Detalles)
            {
                await _inventarioService.RegistrarSalida(
                    detalle.ProductoId!.Value,
                    detalle.Cantidad,
                    TipoMovimientoInventario.SalidaFacturaSri,
                    User.GetUserId(),
                    $"SRI-{documento.NumeroFactura}"
                );
            }

            documento.InventarioProcesado = true;

            await _db.SaveChangesAsync();
            await transaccion.CommitAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            await transaccion.RollbackAsync();
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("detalle/{detalleId:int}/producto/{productoId:int}")]
    public async Task<IActionResult> Relacionar(int detalleId, int productoId)
    {
        var detalle = await _db.DocumentosSriDetalles.FindAsync(detalleId);

        if (detalle is null)
        {
            return NotFound();
        }

        detalle.ProductoId = productoId;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static DateTime ObtenerFechaEmision(string valor)
    {
        var formatos = new[]
        {
            "dd/MM/yyyy",
            "yyyy-MM-dd"
        };

        return DateTime.TryParseExact(
            valor,
            formatos,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var fecha
        )
            ? DateTime.SpecifyKind(fecha, DateTimeKind.Local).ToUniversalTime()
            : DateTime.UtcNow;
    }
}
