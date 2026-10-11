using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClinicaAmelita.Api.Entities;
using Microsoft.IdentityModel.Tokens;

namespace ClinicaAmelita.Api.Services;

public class JwtService(IConfiguration configuration)
{
    public string Generate(Usuario usuario, IEnumerable<string> roles, IEnumerable<string> permisos)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new("nombre", $"{usuario.Nombre} {usuario.Apellido}".Trim())
        };
        claims.AddRange(roles.Distinct().Select(x => new Claim(ClaimTypes.Role, x)));
        claims.AddRange(permisos.Distinct().Select(x => new Claim("permiso", x)));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var token = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims,
            expires: DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationMinutes")),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
