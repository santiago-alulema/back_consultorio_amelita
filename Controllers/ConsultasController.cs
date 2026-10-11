using ClinicaAmelita.Api.Data;
using ClinicaAmelita.Api.Entities;
using ClinicaAmelita.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAmelita.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/consultas")]
public class ConsultasController : ControllerBase
{
    private readonly AppDbContext _db;

    public ConsultasController(AppDbContext db)
    {
        _db = db;
    }

    public record ConsultaRequest(
        int PacienteId,
        DateTime FechaConsulta,
        string MotivoConsulta,
        string? EnfermedadActual,
        decimal? PesoKg,
        decimal? TallaCm,
        decimal? Temperatura,
        string? PresionArterial,
        int? FrecuenciaCardiaca,
        int? SaturacionOxigeno,
        string? ExamenFisico,
        string? Diagnostico,
        string? Tratamiento,
        string? Indicaciones,
        string? Observaciones,
        decimal ValorConsulta,
        string? FormaPago,
        int? CitaId
    );

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int? pacienteId)
    {
        var query = _db.Consultas
            .AsNoTracking()
            .Where(x => x.EsActivo);

        if (pacienteId.HasValue)
        {
            query = query.Where(x => x.PacienteId == pacienteId.Value);
        }

        if (desde.HasValue)
        {
            query = query.Where(x => x.FechaConsulta >= desde.Value.ToUniversalTime());
        }

        if (hasta.HasValue)
        {
            query = query.Where(x => x.FechaConsulta < hasta.Value.ToUniversalTime().AddDays(1));
        }

        var consultas = await query
            .OrderByDescending(x => x.FechaConsulta)
            .Select(x => new
            {
                x.Id,
                x.PacienteId,
                Paciente = x.Paciente.Nombres + " " + x.Paciente.Apellidos,
                x.FechaConsulta,
                x.MotivoConsulta,
                x.Diagnostico,
                x.ValorConsulta,
                x.FormaPago,
                Medico = x.MedicoUsuario.Nombre + " " + x.MedicoUsuario.Apellido,
                RecetaId = x.Recetas
                    .Where(r => r.EsActivo)
                    .Select(r => (int?)r.Id)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(consultas);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ById(int id)
    {
        var consulta = await _db.Consultas
            .AsNoTracking()
            .Where(x => x.Id == id && x.EsActivo)
            .Select(x => new
            {
                x.Id,
                x.PacienteId,
                Paciente = x.Paciente.Nombres + " " + x.Paciente.Apellidos,
                x.MedicoUsuarioId,
                Medico = x.MedicoUsuario.Nombre + " " + x.MedicoUsuario.Apellido,
                x.FechaConsulta,
                x.MotivoConsulta,
                x.EnfermedadActual,
                x.PesoKg,
                x.TallaCm,
                x.Temperatura,
                x.PresionArterial,
                x.FrecuenciaCardiaca,
                x.SaturacionOxigeno,
                x.ExamenFisico,
                x.Diagnostico,
                x.Tratamiento,
                x.Indicaciones,
                x.Observaciones,
                x.ValorConsulta,
                x.FormaPago,
                RecetaId = x.Recetas
                    .Where(r => r.EsActivo)
                    .Select(r => (int?)r.Id)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (consulta is null)
        {
            return NotFound();
        }

        return Ok(consulta);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ConsultaRequest request)
    {
        var validacion = await ValidarAsync(request);

        if (validacion is not null)
        {
            return BadRequest(validacion);
        }

        var usuarioId = User.GetUserId();
        var consulta = new Consulta
        {
            PacienteId = request.PacienteId,
            MedicoUsuarioId = usuarioId,
            FechaConsulta = ConvertirFechaUtc(request.FechaConsulta),
            MotivoConsulta = request.MotivoConsulta.Trim(),
            EnfermedadActual = request.EnfermedadActual,
            PesoKg = request.PesoKg,
            TallaCm = request.TallaCm,
            Temperatura = request.Temperatura,
            PresionArterial = request.PresionArterial,
            FrecuenciaCardiaca = request.FrecuenciaCardiaca,
            SaturacionOxigeno = request.SaturacionOxigeno,
            ExamenFisico = request.ExamenFisico,
            Diagnostico = request.Diagnostico,
            Tratamiento = request.Tratamiento,
            Indicaciones = request.Indicaciones,
            Observaciones = request.Observaciones,
            ValorConsulta = request.ValorConsulta,
            FormaPago = string.IsNullOrWhiteSpace(request.FormaPago)
                ? "Efectivo"
                : request.FormaPago,
            UsuarioCreacionId = usuarioId
        };

        _db.Consultas.Add(consulta);
        await _db.SaveChangesAsync();

        if (request.CitaId.HasValue)
        {
            var cita = await _db.Citas
                .FirstAsync(x => x.Id == request.CitaId.Value && x.EsActivo);

            cita.ConsultaId = consulta.Id;
            cita.Estado = EstadoCita.Atendida;
            cita.UsuarioActualizacionId = usuarioId;

            await _db.SaveChangesAsync();
        }

        return Ok(new
        {
            consulta.Id
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ConsultaRequest request)
    {
        var consulta = await _db.Consultas
            .FirstOrDefaultAsync(x => x.Id == id && x.EsActivo);

        if (consulta is null)
        {
            return NotFound();
        }

        var validacion = await ValidarAsync(request);

        if (validacion is not null)
        {
            return BadRequest(validacion);
        }

        consulta.PacienteId = request.PacienteId;
        consulta.FechaConsulta = ConvertirFechaUtc(request.FechaConsulta);
        consulta.MotivoConsulta = request.MotivoConsulta.Trim();
        consulta.EnfermedadActual = request.EnfermedadActual;
        consulta.PesoKg = request.PesoKg;
        consulta.TallaCm = request.TallaCm;
        consulta.Temperatura = request.Temperatura;
        consulta.PresionArterial = request.PresionArterial;
        consulta.FrecuenciaCardiaca = request.FrecuenciaCardiaca;
        consulta.SaturacionOxigeno = request.SaturacionOxigeno;
        consulta.ExamenFisico = request.ExamenFisico;
        consulta.Diagnostico = request.Diagnostico;
        consulta.Tratamiento = request.Tratamiento;
        consulta.Indicaciones = request.Indicaciones;
        consulta.Observaciones = request.Observaciones;
        consulta.ValorConsulta = request.ValorConsulta;
        consulta.FormaPago = string.IsNullOrWhiteSpace(request.FormaPago)
            ? "Efectivo"
            : request.FormaPago;
        consulta.UsuarioActualizacionId = User.GetUserId();

        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string?> ValidarAsync(ConsultaRequest request)
    {
        var pacienteExiste = await _db.Pacientes
            .AnyAsync(x => x.Id == request.PacienteId && x.EsActivo);

        if (!pacienteExiste)
        {
            return "El paciente seleccionado no existe.";
        }

        if (string.IsNullOrWhiteSpace(request.MotivoConsulta))
        {
            return "El motivo de consulta es obligatorio.";
        }

        if (request.CitaId.HasValue)
        {
            var citaValida = await _db.Citas.AnyAsync(x =>
                x.Id == request.CitaId.Value
                && x.PacienteId == request.PacienteId
                && x.EsActivo
                && x.Estado != EstadoCita.Anulada
            );

            if (!citaValida)
            {
                return "La cita seleccionada no corresponde al paciente.";
            }
        }

        return null;
    }

    private static DateTime ConvertirFechaUtc(DateTime fecha)
    {
        if (fecha.Kind == DateTimeKind.Utc)
        {
            return fecha;
        }

        if (fecha.Kind == DateTimeKind.Local)
        {
            return fecha.ToUniversalTime();
        }

        return DateTime.SpecifyKind(fecha, DateTimeKind.Local).ToUniversalTime();
    }
}
