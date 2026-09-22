using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Diaspora.Tests.Client.Api.Controllers.Auth;

public class RegisterControllerTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithValidPayload_Returns201WithAccessToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", ValidPayload($"{Guid.NewGuid()}@example.com"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrEmpty(body.GetProperty("accessToken").GetString()));
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", ValidPayload(email));

        var response = await _client.PostAsJsonAsync("/api/auth/register", ValidPayload(email));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidPayload_Returns400WithFieldErrors()
    {
        var payload = new
        {
            email = "not-an-email",
            password = "short",
            confirmPassword = "different",
            firstName = "",
            lastName = "Doe",
            dateOfBirth = "1998-04-12",
            location = "Berlin",
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").TryGetProperty("email", out _));
    }

    private object ValidPayload(string email) => new
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
