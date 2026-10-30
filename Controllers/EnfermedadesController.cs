using ClinicaAmelita.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAmelita.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/enfermedades")]
public class EnfermedadesController(AppDbContext db) : ControllerBase
{
    [HttpGet("categorias")]
    public async Task<IActionResult> Categorias()
    {
        var categorias = await db.Enfermedades.AsNoTracking()
            .Select(x => new { Codigo = x.CategoriaCodigo, Nombre = x.Categoria })
            .Distinct()
            .OrderBy(x => x.Codigo)
            .ToListAsync();

        return Ok(categorias);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria))
        {
            return BadRequest("Seleccione una categoría.");
        }

        var enfermedades = await db.Enfermedades.AsNoTracking()
            .Where(x => x.CategoriaCodigo == categoria)
            .OrderBy(x => x.Codigo)
            .Select(x => new { x.Codigo, x.Nombre })
            .ToListAsync();

        return Ok(enfermedades);
    }
}
