using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Diaspora.Tests.Client.Api.Controllers.Auth;

public class LogoutControllerTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    [Fact]
    public async Task Logout_WithNoCookie_Returns204()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithActiveCookie_Returns204_AndSubsequentRefreshFails()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", RegisterPayload($"{Guid.NewGuid()}@example.com"));
        registerResponse.EnsureSuccessStatusCode();

        var logoutResponse = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var refreshResponse = await client.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_CalledTwiceWithTheSameCookie_IsIdempotent()
    {
        var seedClient = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var registerResponse = await seedClient.PostAsJsonAsync("/api/auth/register", RegisterPayload($"{Guid.NewGuid()}@example.com"));
        registerResponse.EnsureSuccessStatusCode();
        var cookieValue = ExtractCookieValue(
            registerResponse.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("refreshToken=", StringComparison.Ordinal)));

        Assert.Equal(HttpStatusCode.NoContent, await LogoutWithCookieAsync(cookieValue));
        Assert.Equal(HttpStatusCode.NoContent, await LogoutWithCookieAsync(cookieValue));
    }

    private async Task<HttpStatusCode> LogoutWithCookieAsync(string cookieValue)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        request.Headers.Add("Cookie", $"refreshToken={cookieValue}");
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var response = await client.SendAsync(request);
        return response.StatusCode;
    }

    private static string ExtractCookieValue(string setCookieHeader) =>
        setCookieHeader.Split(';')[0].Split('=', 2)[1];

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
