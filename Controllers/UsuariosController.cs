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
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordService _passwordService;

    public UsuariosController(AppDbContext db, PasswordService passwordService)
    {
        _db = db;
        _passwordService = passwordService;
    }

    public record UsuarioRequest(
        string Nombre,
        string Apellido,
        string NombreUsuario,
        string? Email,
        string? Password,
        List<int> Roles
    );

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool incluirInactivos = true)
    {
        var query = _db.Usuarios.AsNoTracking().AsQueryable();

        if (!incluirInactivos)
        {
            query = query.Where(x => x.EsActivo);
        }

        var usuarios = await query
            .OrderBy(x => x.Nombre)
            .ThenBy(x => x.Apellido)
            .Select(x => new
            {
                x.Id,
                x.Nombre,
                x.Apellido,
                x.NombreUsuario,
                x.Email,
                x.EsActivo,
                Roles = x.UsuarioRoles.Select(r => new
                {
                    r.RolId,
                    r.Rol.Nombre
                })
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    [HttpPost]
    public async Task<IActionResult> Create(UsuarioRequest request)
    {
        var error = await ValidarAsync(request);

        if (error is not null)
        {
            return BadRequest(error);
        }

        var (hash, salt) = _passwordService.HashPassword(
            request.Password ?? "Temporal123*"
        );

        var usuario = new Usuario
        {
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            NombreUsuario = request.NombreUsuario.Trim(),
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            UsuarioCreacionId = User.GetUserId()
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        AgregarRoles(usuario.Id, request.Roles);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            usuario.Id
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UsuarioRequest request)
    {
        var usuario = await _db.Usuarios
            .Include(x => x.UsuarioRoles)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (usuario is null)
        {
            return NotFound();
        }

        var error = await ValidarAsync(request, id);

        if (error is not null)
        {
            return BadRequest(error);
        }

        usuario.Nombre = request.Nombre.Trim();
        usuario.Apellido = request.Apellido.Trim();
        usuario.NombreUsuario = request.NombreUsuario.Trim();
        usuario.Email = request.Email;
        usuario.UsuarioActualizacionId = User.GetUserId();

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var (hash, salt) = _passwordService.HashPassword(request.Password);
            usuario.PasswordHash = hash;
            usuario.PasswordSalt = salt;
        }

        _db.UsuariosRoles.RemoveRange(usuario.UsuarioRoles);
        AgregarRoles(usuario.Id, request.Roles);

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);

        if (usuario is null)
        {
            return NotFound();
        }

        if (usuario.Id == User.GetUserId() && usuario.EsActivo)
        {
            return BadRequest("No puede desactivar su propio usuario mientras está autenticado.");
        }

        usuario.EsActivo = !usuario.EsActivo;
        usuario.UsuarioActualizacionId = User.GetUserId();

        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string?> ValidarAsync(UsuarioRequest request, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre)
            || string.IsNullOrWhiteSpace(request.Apellido)
            || string.IsNullOrWhiteSpace(request.NombreUsuario))
        {
            return "Nombre, apellido y usuario son obligatorios.";
        }

        var existe = await _db.Usuarios.AnyAsync(x =>
            x.NombreUsuario == request.NombreUsuario.Trim()
            && (!id.HasValue || x.Id != id.Value)
        );

        if (existe)
        {
            return "El nombre de usuario ya existe.";
        }

        return null;
    }

    private void AgregarRoles(int usuarioId, IEnumerable<int> roles)
    {
        _db.UsuariosRoles.AddRange(
            roles
                .Distinct()
                .Select(rolId => new UsuarioRol
                {
                    UsuarioId = usuarioId,
                    RolId = rolId
                })
        );
    }
}
