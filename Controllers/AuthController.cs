using ClinicaAmelita.Api.Data;
using ClinicaAmelita.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAmelita.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordService _passwordService;
    private readonly JwtService _jwtService;

    public AuthController(
        AppDbContext db,
        PasswordService passwordService,
        JwtService jwtService)
    {
        _db = db;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public record LoginRequest(string Usuario, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var usuario = await _db.Usuarios
            .Include(x => x.UsuarioRoles)
            .ThenInclude(x => x.Rol)
            .ThenInclude(x => x.RolPermisos)
            .ThenInclude(x => x.Permiso)
            .FirstOrDefaultAsync(x =>
                x.NombreUsuario == request.Usuario
                && x.EsActivo
            );

        if (
            usuario is null
            || !_passwordService.Verify(
                request.Password,
                usuario.PasswordHash,
                usuario.PasswordSalt
            )
        )
        {
            return Unauthorized("Usuario o contraseña incorrectos.");
        }

        var roles = usuario.UsuarioRoles
            .Where(x => x.Rol.EsActivo)
            .Select(x => x.Rol.Nombre)
            .Distinct()
            .ToList();

        var permisos = usuario.UsuarioRoles
            .Where(x => x.Rol.EsActivo)
            .SelectMany(x => x.Rol.RolPermisos)
            .Where(x => x.Permiso.EsActivo)
            .Select(x => x.Permiso.Codigo)
            .Distinct()
            .ToList();

        return Ok(new
        {
            token = _jwtService.Generate(usuario, roles, permisos),
            usuario = new
            {
                usuario.Id,
                usuario.NombreUsuario,
                nombre = $"{usuario.Nombre} {usuario.Apellido}".Trim(),
                roles,
                permisos
            }
        });
    }
}
