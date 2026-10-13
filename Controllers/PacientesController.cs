using ClinicaAmelita.Api.Data;
using ClinicaAmelita.Api.Entities;
using ClinicaAmelita.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAmelita.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/pacientes")]
public class PacientesController : ControllerBase
{
    private readonly AppDbContext _db;

    public PacientesController(AppDbContext db)
    {
        _db = db;
    }

    public record PacienteRequest(
        string Identificacion,
        string Nombres,
        string Apellidos,
        DateOnly? FechaNacimiento,
        string? Sexo,
        string? TipoSangre,
        string? Telefono,
        string? Email,
        string? Direccion,
        string? ContactoEmergencia,
        string? TelefonoEmergencia
    );

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? buscar,
        [FromQuery] bool incluirInactivos = false)
    {
        var query = _db.Pacientes.AsNoTracking().AsQueryable();

        if (!incluirInactivos)
        {
            query = query.Where(x => x.EsActivo);
        }

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim().ToLower();

            query = query.Where(x =>
                x.Identificacion.Contains(buscar.Trim())
                || x.Nombres.ToLower().Contains(texto)
                || x.Apellidos.ToLower().Contains(texto)
            );
        }

        var pacientes = await query
            .OrderBy(x => x.Apellidos)
            .ThenBy(x => x.Nombres)
            .Take(200)
            .Select(x => new
            {
                x.Id,
                x.Identificacion,
                x.Nombres,
                x.Apellidos,
                x.FechaNacimiento,
                x.Sexo,
                x.TipoSangre,
                x.Telefono,
                x.Email,
                x.Direccion,
                x.ContactoEmergencia,
                x.TelefonoEmergencia,
                x.EsActivo
            })
            .ToListAsync();

        return Ok(pacientes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var paciente = await _db.Pacientes
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Identificacion,
                x.Nombres,
                x.Apellidos,
                x.FechaNacimiento,
                x.Sexo,
                x.TipoSangre,
                x.Telefono,
                x.Email,
                x.Direccion,
                x.ContactoEmergencia,
                x.TelefonoEmergencia,
                x.EsActivo
            })
            .FirstOrDefaultAsync();

        return paciente is null
            ? NotFound()
            : Ok(paciente);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PacienteRequest request)
    {
        var error = await ValidarAsync(request);

        if (error is not null)
        {
            return BadRequest(error);
        }

        var paciente = new Paciente
        {
            Identificacion = request.Identificacion.Trim(),
            Nombres = request.Nombres.Trim(),
            Apellidos = request.Apellidos.Trim(),
            FechaNacimiento = request.FechaNacimiento,
            Sexo = request.Sexo,
            TipoSangre = request.TipoSangre,
            Telefono = request.Telefono,
            Email = request.Email,
            Direccion = request.Direccion,
            ContactoEmergencia = request.ContactoEmergencia,
            TelefonoEmergencia = request.TelefonoEmergencia,
            UsuarioCreacionId = User.GetUserId()
        };

        _db.Pacientes.Add(paciente);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            paciente.Id
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, PacienteRequest request)
    {
        var paciente = await _db.Pacientes.FindAsync(id);

        if (paciente is null)
        {
            return NotFound();
        }

        var error = await ValidarAsync(request, id);

        if (error is not null)
        {
            return BadRequest(error);
        }

        paciente.Identificacion = request.Identificacion.Trim();
        paciente.Nombres = request.Nombres.Trim();
        paciente.Apellidos = request.Apellidos.Trim();
        paciente.FechaNacimiento = request.FechaNacimiento;
        paciente.Sexo = request.Sexo;
        paciente.TipoSangre = request.TipoSangre;
        paciente.Telefono = request.Telefono;
        paciente.Email = request.Email;
        paciente.Direccion = request.Direccion;
        paciente.ContactoEmergencia = request.ContactoEmergencia;
        paciente.TelefonoEmergencia = request.TelefonoEmergencia;
        paciente.UsuarioActualizacionId = User.GetUserId();

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var paciente = await _db.Pacientes.FindAsync(id);

        if (paciente is null)
        {
            return NotFound();
        }

        paciente.EsActivo = !paciente.EsActivo;
        paciente.UsuarioActualizacionId = User.GetUserId();

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id:int}/baja")]
    public async Task<IActionResult> Baja(int id)
    {
        var paciente = await _db.Pacientes.FindAsync(id);

        if (paciente is null)
        {
            return NotFound();
        }

        paciente.EsActivo = false;
        paciente.UsuarioActualizacionId = User.GetUserId();

        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string?> ValidarAsync(PacienteRequest request, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(request.Identificacion))
        {
            return "La identificación es obligatoria.";
        }

        if (string.IsNullOrWhiteSpace(request.Nombres) || string.IsNullOrWhiteSpace(request.Apellidos))
        {
            return "Los nombres y apellidos son obligatorios.";
        }

        var existe = await _db.Pacientes.AnyAsync(x =>
            x.Identificacion == request.Identificacion.Trim()
            && (!id.HasValue || x.Id != id.Value)
        );

        return existe
            ? "Ya existe un paciente con esa identificación."
            : null;
    }
}
