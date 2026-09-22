using Diaspora.Identity.Infrastructure.Authentication;

namespace Diaspora.Tests.Modules.Identity.Infrastructure.Authentication;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_ThenVerify_WithCorrectPassword_Succeeds()
    {
        var hashed = _sut.Hash("Str0ngPass1");

        var result = _sut.Verify("Str0ngPass1", hashed.Hash, hashed.Salt, hashed.Iterations);

        Assert.True(result);
    }

    [Fact]
    public void Hash_ThenVerify_WithWrongPassword_Fails()
    {
        var hashed = _sut.Hash("Str0ngPass1");

        var result = _sut.Verify("WrongPass1", hashed.Hash, hashed.Salt, hashed.Iterations);

        Assert.False(result);
    }

    [Fact]
    public void Hash_CalledTwiceForSamePassword_ProducesDifferentSaltsAndHashes()
    {
        var first = _sut.Hash("Str0ngPass1");
        var second = _sut.Hash("Str0ngPass1");

        Assert.NotEqual(first.Salt, second.Salt);
        Assert.NotEqual(first.Hash, second.Hash);
    }
}
