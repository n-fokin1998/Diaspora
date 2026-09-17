namespace Identity.Application.Common.Abstractions;

public interface IPasswordHasher
{
    HashedPassword Hash(string password);

    bool Verify(string password, byte[] hash, byte[] salt, int iterations);
}

public sealed record HashedPassword(byte[] Hash, byte[] Salt, int Iterations);
