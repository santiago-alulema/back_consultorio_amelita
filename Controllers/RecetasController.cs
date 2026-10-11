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
[Route("api/recetas")]
public class RecetasController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly RecetaPdfService _recetaPdfService;

    public RecetasController(AppDbContext db, RecetaPdfService recetaPdfService)
    {
        _db = db;
        _recetaPdfService = recetaPdfService;
    }

    public record DetalleRequest(
        int? ProductoId,
        string Medicamento,
        string? Presentacion,
        string Dosis,
        string Frecuencia,
        string Duracion,
        decimal? Cantidad,
        string? Indicaciones
    );

    public record RecetaRequest(
        int PacienteId,
        int? ConsultaId,
        string? IndicacionesGenerales,
        List<DetalleRequest> Detalles
    );

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? pacienteId)
    {
        var recetas = await _db.Recetas
            .AsNoTracking()
            .Where(x => x.EsActivo && (!pacienteId.HasValue || x.PacienteId == pacienteId.Value))
            .OrderByDescending(x => x.FechaEmision)
            .Select(x => new
            {
                x.Id,
                x.PacienteId,
                x.ConsultaId,
                Paciente = x.Paciente.Nombres + " " + x.Paciente.Apellidos,
                x.FechaEmision,
                Medico = x.MedicoUsuario.Nombre + " " + x.MedicoUsuario.Apellido,
                x.IndicacionesGenerales,
                Detalles = x.Detalles
                    .Where(d => d.EsActivo)
                    .Select(d => new
                    {
                        d.Id,
                        d.ProductoId,
                        d.Medicamento,
                        d.Presentacion,
                        d.Dosis,
                        d.Frecuencia,
                        d.Duracion,
                        d.Cantidad,
                        d.Indicaciones
                    })
            })
            .ToListAsync();

        return Ok(recetas);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ById(int id)
    {
        var receta = await _db.Recetas
            .AsNoTracking()
            .Where(x => x.Id == id && x.EsActivo)
            .Select(x => new
            {
                x.Id,
                x.PacienteId,
                x.ConsultaId,
                Paciente = new
                {
                    x.Paciente.Id,
                    x.Paciente.Identificacion,
                    x.Paciente.Nombres,
                    x.Paciente.Apellidos,
                    x.Paciente.FechaNacimiento,
                    x.Paciente.Telefono
                },
                x.FechaEmision,
                Medico = new
                {
                    x.MedicoUsuario.Id,
                    x.MedicoUsuario.Nombre,
                    x.MedicoUsuario.Apellido
                },
                x.IndicacionesGenerales,
                Detalles = x.Detalles
                    .Where(d => d.EsActivo)
                    .Select(d => new
                    {
                        d.Id,
                        d.ProductoId,
                        d.Medicamento,
                        d.Presentacion,
                        d.Dosis,
                        d.Frecuencia,
                        d.Duracion,
                        d.Cantidad,
                        d.Indicaciones
                    })
            })
            .FirstOrDefaultAsync();

        if (receta is null)
        {
            return NotFound();
        }

        return Ok(receta);
    }

    [HttpPost]
    public async Task<IActionResult> Create(RecetaRequest request)
    {
        if (request.Detalles.Count == 0)
        {
            return BadRequest("La receta debe tener al menos un medicamento.");
        }

        var pacienteExiste = await _db.Pacientes
            .AnyAsync(x => x.Id == request.PacienteId && x.EsActivo);

        if (!pacienteExiste)
        {
            return BadRequest("El paciente seleccionado no existe.");
        }

        if (request.ConsultaId.HasValue)
        {
            var consultaValida = await _db.Consultas.AnyAsync(x =>
                x.Id == request.ConsultaId.Value
                && x.PacienteId == request.PacienteId
                && x.EsActivo
            );

            if (!consultaValida)
            {
                return BadRequest("La consulta no corresponde al paciente seleccionado.");
            }
        }

        var usuarioId = User.GetUserId();

        var receta = new Receta
        {
            PacienteId = request.PacienteId,
            ConsultaId = request.ConsultaId,
            MedicoUsuarioId = usuarioId,
            IndicacionesGenerales = request.IndicacionesGenerales,
            UsuarioCreacionId = usuarioId,
            Detalles = request.Detalles.Select(d => new RecetaDetalle
            {
                ProductoId = d.ProductoId,
                Medicamento = d.Medicamento.Trim(),
                Presentacion = d.Presentacion,
                Dosis = d.Dosis.Trim(),
                Frecuencia = d.Frecuencia.Trim(),
                Duracion = d.Duracion.Trim(),
                Cantidad = d.Cantidad,
                Indicaciones = d.Indicaciones,
                UsuarioCreacionId = usuarioId
            }).ToList()
        };

        _db.Recetas.Add(receta);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            receta.Id
        });
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> Pdf(int id)
    {
        var receta = await _db.Recetas
            .AsNoTracking()
            .Include(x => x.Paciente)
            .ThenInclude(x => x.HistoriasClinicas)
            .Include(x => x.MedicoUsuario)
            .Include(x => x.Detalles.Where(d => d.EsActivo))
            .FirstOrDefaultAsync(x => x.Id == id && x.EsActivo);

        if (receta is null)
        {
            return NotFound();
        }

        var medicoImprime = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == User.GetUserId() && x.EsActivo);

        if (medicoImprime is null)
        {
            return Unauthorized();
        }

        var pdf = await _recetaPdfService.GenerarAsync(receta, medicoImprime);
        var nombre = $"receta_{receta.Id}_{receta.Paciente.Identificacion}.pdf";

        return File(pdf, "application/pdf", nombre);
    }
}
