using System.Security.Claims;

namespace ClinicaAmelita.Api.Helpers;

public static class UserExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        return int.TryParse(
            user.FindFirstValue(ClaimTypes.NameIdentifier),
            out var id
        )
            ? id
            : 0;
    }
}
