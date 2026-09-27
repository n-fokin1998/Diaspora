using System.Security.Cryptography;
using Diaspora.Identity.Application.Common.Abstractions;

namespace Diaspora.Identity.Infrastructure.Authentication;

internal class PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;
    private const int Iterations = 600_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public HashedPassword Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSizeBytes);
        return new HashedPassword(hash, salt, Iterations);
    }

    public bool Verify(string password, byte[] hash, byte[] salt, int iterations)
    {
        var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, hash.Length);
        return CryptographicOperations.FixedTimeEquals(computedHash, hash);
    }
}
