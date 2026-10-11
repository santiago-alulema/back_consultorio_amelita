using ClinicaAmelita.Api.Data;
using ClinicaAmelita.Api.Entities;
using ClinicaAmelita.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAmelita.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/historias-clinicas")]
public class HistoriasClinicasController : ControllerBase
{
    private readonly AppDbContext _db;

    public HistoriasClinicasController(AppDbContext db)
    {
        _db = db;
    }

    public record HistoriaClinicaRequest(
        int PacienteId,
        string? Alergias,
        string? AntecedentesPersonales,
        string? AntecedentesFamiliares,
        string? Cirugias,
        string? MedicacionHabitual,
        string? Observaciones
    );

    [HttpGet("paciente/{pacienteId:int}")]
    public async Task<IActionResult> PorPaciente(int pacienteId)
    {
        var pacienteExiste = await _db.Pacientes
            .AnyAsync(x => x.Id == pacienteId && x.EsActivo);

        if (!pacienteExiste)
        {
            return NotFound("El paciente no existe.");
        }

        var historias = await _db.HistoriasClinicas
            .AsNoTracking()
            .Where(x => x.PacienteId == pacienteId && x.EsActivo)
            .OrderByDescending(x => x.FechaCreacion)
            .Select(x => new
            {
                x.Id,
                x.PacienteId,
                x.Alergias,
                x.AntecedentesPersonales,
                x.AntecedentesFamiliares,
                x.Cirugias,
                x.MedicacionHabitual,
                x.Observaciones,
                x.FechaCreacion,
                x.FechaActualizacion
            })
            .ToListAsync();

        return Ok(historias);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ById(int id)
    {
        var historia = await _db.HistoriasClinicas
            .AsNoTracking()
            .Where(x => x.Id == id && x.EsActivo)
            .Select(x => new
            {
                x.Id,
                x.PacienteId,
                x.Alergias,
                x.AntecedentesPersonales,
                x.AntecedentesFamiliares,
                x.Cirugias,
                x.MedicacionHabitual,
                x.Observaciones,
                x.FechaCreacion,
                x.FechaActualizacion
            })
            .FirstOrDefaultAsync();

        if (historia is null)
        {
            return NotFound();
        }

        return Ok(historia);
    }

    [HttpPost]
    public async Task<IActionResult> Create(HistoriaClinicaRequest request)
    {
        var pacienteExiste = await _db.Pacientes
            .AnyAsync(x => x.Id == request.PacienteId && x.EsActivo);

        if (!pacienteExiste)
        {
            return BadRequest("El paciente seleccionado no existe.");
        }

        var historia = new HistoriaClinica
        {
            PacienteId = request.PacienteId,
            Alergias = request.Alergias,
            AntecedentesPersonales = request.AntecedentesPersonales,
            AntecedentesFamiliares = request.AntecedentesFamiliares,
            Cirugias = request.Cirugias,
            MedicacionHabitual = request.MedicacionHabitual,
            Observaciones = request.Observaciones,
            UsuarioCreacionId = User.GetUserId()
        };

        _db.HistoriasClinicas.Add(historia);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            historia.Id
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, HistoriaClinicaRequest request)
    {
        var historia = await _db.HistoriasClinicas
            .FirstOrDefaultAsync(x => x.Id == id && x.EsActivo);

        if (historia is null)
        {
            return NotFound();
        }

        historia.PacienteId = request.PacienteId;
        historia.Alergias = request.Alergias;
        historia.AntecedentesPersonales = request.AntecedentesPersonales;
        historia.AntecedentesFamiliares = request.AntecedentesFamiliares;
        historia.Cirugias = request.Cirugias;
        historia.MedicacionHabitual = request.MedicacionHabitual;
        historia.Observaciones = request.Observaciones;
        historia.UsuarioActualizacionId = User.GetUserId();

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var historia = await _db.HistoriasClinicas
            .FirstOrDefaultAsync(x => x.Id == id && x.EsActivo);

        if (historia is null)
        {
            return NotFound();
        }

        historia.EsActivo = false;
        historia.UsuarioActualizacionId = User.GetUserId();

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
