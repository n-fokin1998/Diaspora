using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Diaspora.Tests.Client.Api.Controllers.Auth;

public class LoginControllerTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_WithCorrectCredentials_Returns200WithAccessToken()
    {
        var email = await RegisterAndReturnEmailAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "Str0ngPass1" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrEmpty(body.GetProperty("accessToken").GetString()));
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var email = await RegisterAndReturnEmailAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "WrongPassword1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnregisteredEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = $"{Guid.NewGuid()}@example.com", password = "Str0ngPass1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithMissingPassword_Returns400()
    {
        var email = await RegisterAndReturnEmailAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<string> RegisterAndReturnEmailAsync()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        var response = await _client.PostAsJsonAsync("/api/auth/register", RegisterPayload(email));
        response.EnsureSuccessStatusCode();
        return email;
    }

    private object RegisterPayload(string email) => new
    {
        email,
        password = "Str0ngPass1",
        confirmPassword = "Str0ngPass1",
        firstName = "Jane",
        lastName = "Doe",
        dateOfBirth = "1998-04-12",
        location = "Berlin, Germany",
    };
}
