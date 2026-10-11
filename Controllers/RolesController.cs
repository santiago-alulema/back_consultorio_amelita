using ClinicaAmelita.Api.Data;
using ClinicaAmelita.Api.Entities;
using ClinicaAmelita.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAmelita.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RolesController(AppDbContext db)
    {
        _db = db;
    }

    public record RolRequest(string Nombre, string? Descripcion, List<int> Permisos);

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool incluirInactivos = false)
    {
        var query = _db.Roles.AsNoTracking().AsQueryable();

        if (!incluirInactivos)
        {
            query = query.Where(x => x.EsActivo);
        }

        var roles = await query
            .OrderBy(x => x.Nombre)
            .Select(x => new
            {
                x.Id,
                x.Nombre,
                x.Descripcion,
                x.EsActivo,
                Permisos = x.RolPermisos.Select(p => new
                {
                    p.PermisoId,
                    p.Permiso.Codigo,
                    p.Permiso.Nombre,
                    p.Permiso.Modulo
                })
            })
            .ToListAsync();

        return Ok(roles);
    }

    [HttpGet("permisos")]
    public async Task<IActionResult> Permisos()
    {
        var permisos = await _db.Permisos
            .AsNoTracking()
            .Where(x => x.EsActivo)
            .OrderBy(x => x.Modulo)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

        return Ok(permisos);
    }

    [HttpPost]
    public async Task<IActionResult> Create(RolRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest("El nombre del rol es obligatorio.");
        }

        var existe = await _db.Roles.AnyAsync(x => x.Nombre == request.Nombre.Trim());

        if (existe)
        {
            return BadRequest("Ya existe un rol con ese nombre.");
        }

        var usuarioId = User.GetUserId();
        var rol = new Rol
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion,
            UsuarioCreacionId = usuarioId
        };

        _db.Roles.Add(rol);
        await _db.SaveChangesAsync();

        AgregarPermisos(rol.Id, request.Permisos);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            rol.Id
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, RolRequest request)
    {
        var rol = await _db.Roles
            .Include(x => x.RolPermisos)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (rol is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest("El nombre del rol es obligatorio.");
        }

        var existe = await _db.Roles.AnyAsync(x =>
            x.Id != id
            && x.Nombre == request.Nombre.Trim()
        );

        if (existe)
        {
            return BadRequest("Ya existe un rol con ese nombre.");
        }

        rol.Nombre = request.Nombre.Trim();
        rol.Descripcion = request.Descripcion;
        rol.UsuarioActualizacionId = User.GetUserId();

        _db.RolesPermisos.RemoveRange(rol.RolPermisos);
        AgregarPermisos(rol.Id, request.Permisos);

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var rol = await _db.Roles.FindAsync(id);

        if (rol is null)
        {
            return NotFound();
        }

        if (rol.Nombre == "Administrador" && rol.EsActivo)
        {
            return BadRequest("El rol Administrador no puede ser desactivado.");
        }

        rol.EsActivo = !rol.EsActivo;
        rol.UsuarioActualizacionId = User.GetUserId();

        await _db.SaveChangesAsync();

        return NoContent();
    }

    private void AgregarPermisos(int rolId, IEnumerable<int> permisos)
    {
        _db.RolesPermisos.AddRange(
            permisos
                .Distinct()
                .Select(permisoId => new RolPermiso
                {
                    RolId = rolId,
                    PermisoId = permisoId
                })
        );
    }
}
