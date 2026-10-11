using System.Security.Cryptography;

namespace ClinicaAmelita.Api.Services;

public class PasswordService
{
    public (string Hash, string Salt) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(32);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 120000, HashAlgorithmName.SHA512, 64);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool Verify(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var expected = Convert.FromBase64String(hash);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, 120000, HashAlgorithmName.SHA512, 64);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
