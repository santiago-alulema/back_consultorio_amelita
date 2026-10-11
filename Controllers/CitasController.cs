using ClinicaAmelita.Api.Data;
using ClinicaAmelita.Api.Entities;
using ClinicaAmelita.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAmelita.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/citas")]
public class CitasController : ControllerBase
{
    private readonly AppDbContext _db;

    public CitasController(AppDbContext db)
    {
        _db = db;
    }

    public record CitaRequest(
        int PacienteId,
        int? MedicoUsuarioId,
        DateTime FechaHoraInicio,
        DateTime? FechaHoraFin,
        string Motivo,
        string? Observaciones
    );

    public record EstadoRequest(EstadoCita Estado);

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var query = _db.Citas
            .AsNoTracking()
            .Where(x => x.EsActivo);

        if (desde.HasValue)
        {
            query = query.Where(x => x.FechaHoraInicio >= desde.Value.ToUniversalTime());
        }

        if (hasta.HasValue)
        {
            query = query.Where(x => x.FechaHoraInicio < hasta.Value.ToUniversalTime().AddDays(1));
        }

        var citas = await query
            .OrderBy(x => x.FechaHoraInicio)
            .Select(x => new
            {
                x.Id,
                x.PacienteId,
                Paciente = x.Paciente.Nombres + " " + x.Paciente.Apellidos,
                x.Paciente.Telefono,
                x.MedicoUsuarioId,
                Medico = x.MedicoUsuario == null
                    ? null
                    : x.MedicoUsuario.Nombre + " " + x.MedicoUsuario.Apellido,
                x.FechaHoraInicio,
                x.FechaHoraFin,
                x.Motivo,
                x.Observaciones,
                Estado = x.Estado.ToString(),
                EstadoId = (int)x.Estado,
                x.ConsultaId
            })
            .ToListAsync();

        return Ok(citas);
    }

    [HttpGet("medicos")]
    public async Task<IActionResult> Medicos()
    {
        var medicos = await _db.Usuarios
            .AsNoTracking()
            .Where(x =>
                x.EsActivo
                && x.UsuarioRoles.Any(r => r.Rol.EsActivo && r.Rol.Nombre == "Medico")
            )
            .OrderBy(x => x.Nombre)
            .Select(x => new
            {
                x.Id,
                Nombre = x.Nombre + " " + x.Apellido
            })
            .ToListAsync();

        return Ok(medicos);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CitaRequest request)
    {
        var error = await ValidarRequestAsync(request);

        if (error is not null)
        {
            return BadRequest(error);
        }

        var usuarioId = User.GetUserId();
        var cita = new Cita
        {
            PacienteId = request.PacienteId,
            MedicoUsuarioId = request.MedicoUsuarioId,
            FechaHoraInicio = ConvertirUtc(request.FechaHoraInicio),
            FechaHoraFin = request.FechaHoraFin.HasValue
                ? ConvertirUtc(request.FechaHoraFin.Value)
                : null,
            Motivo = request.Motivo.Trim(),
            Observaciones = request.Observaciones,
            Estado = EstadoCita.Agendada,
            UsuarioCreacionId = usuarioId
        };

        _db.Citas.Add(cita);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            cita.Id
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CitaRequest request)
    {
        var cita = await _db.Citas
            .FirstOrDefaultAsync(x => x.Id == id && x.EsActivo);

        if (cita is null)
        {
            return NotFound();
        }

        if (cita.Estado is EstadoCita.Atendida or EstadoCita.Anulada)
        {
            return BadRequest("No se puede modificar una cita atendida o anulada.");
        }

        var error = await ValidarRequestAsync(request);

        if (error is not null)
        {
            return BadRequest(error);
        }

        cita.PacienteId = request.PacienteId;
        cita.MedicoUsuarioId = request.MedicoUsuarioId;
        cita.FechaHoraInicio = ConvertirUtc(request.FechaHoraInicio);
        cita.FechaHoraFin = request.FechaHoraFin.HasValue
            ? ConvertirUtc(request.FechaHoraFin.Value)
            : null;
        cita.Motivo = request.Motivo.Trim();
        cita.Observaciones = request.Observaciones;
        cita.UsuarioActualizacionId = User.GetUserId();

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, EstadoRequest request)
    {
        var cita = await _db.Citas
            .FirstOrDefaultAsync(x => x.Id == id && x.EsActivo);

        if (cita is null)
        {
            return NotFound();
        }

        if (request.Estado == EstadoCita.Atendida && !cita.ConsultaId.HasValue)
        {
            return BadRequest("Para marcar la cita como atendida primero debe registrar la consulta.");
        }

        cita.Estado = request.Estado;
        cita.UsuarioActualizacionId = User.GetUserId();

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id:int}/ligar-consulta/{consultaId:int}")]
    public async Task<IActionResult> LigarConsulta(int id, int consultaId)
    {
        var cita = await _db.Citas
            .FirstOrDefaultAsync(x => x.Id == id && x.EsActivo);

        if (cita is null)
        {
            return NotFound();
        }

        var consulta = await _db.Consultas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == consultaId && x.EsActivo);

        if (consulta is null)
        {
            return BadRequest("La consulta no existe.");
        }

        if (consulta.PacienteId != cita.PacienteId)
        {
            return BadRequest("La consulta no corresponde al paciente de la cita.");
        }

        cita.ConsultaId = consulta.Id;
        cita.Estado = EstadoCita.Atendida;
        cita.UsuarioActualizacionId = User.GetUserId();

        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string?> ValidarRequestAsync(CitaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
        {
            return "El motivo de la cita es obligatorio.";
        }

        var pacienteExiste = await _db.Pacientes
            .AnyAsync(x => x.Id == request.PacienteId && x.EsActivo);

        if (!pacienteExiste)
        {
            return "El paciente seleccionado no existe.";
        }

        if (request.MedicoUsuarioId.HasValue)
        {
            var medicoExiste = await _db.Usuarios
                .AnyAsync(x => x.Id == request.MedicoUsuarioId.Value && x.EsActivo);

            if (!medicoExiste)
            {
                return "El médico seleccionado no existe.";
            }
        }

        return null;
    }

    private static DateTime ConvertirUtc(DateTime fecha)
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
