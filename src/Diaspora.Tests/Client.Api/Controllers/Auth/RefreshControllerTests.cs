using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Diaspora.Tests.Client.Api.Controllers.Auth;

public class RefreshControllerTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    // A Secure cookie is only attached automatically by HttpClient's cookie container to an
    // https:// base address — TestServer doesn't perform a real TLS handshake, but the scheme
    // is what the container checks (research.md #10's Secure attribute).
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
    });

    [Fact]
    public async Task Refresh_WithActiveCookie_Returns200AndRotatesCookie()
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", RegisterPayload($"{Guid.NewGuid()}@example.com"));
        registerResponse.EnsureSuccessStatusCode();
        var originalCookie = ExtractSetCookie(registerResponse);

        var refreshResponse = await _client.PostAsync("/api/auth/refresh", null);

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var body = await refreshResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrEmpty(body.GetProperty("accessToken").GetString()));

        var rotatedCookie = ExtractSetCookie(refreshResponse);
        Assert.NotEqual(originalCookie, rotatedCookie);
    }

    [Fact]
    public async Task Refresh_WithNoCookie_Returns401()
    {
        var freshClient = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await freshClient.PostAsync("/api/auth/refresh", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_AfterAlreadyRotated_ReusingOldCookie_Returns401AndRevokesTheCurrentSessionToo()
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", RegisterPayload($"{Guid.NewGuid()}@example.com"));
        registerResponse.EnsureSuccessStatusCode();
        var originalCookieValue = ExtractCookieValue(ExtractSetCookie(registerResponse));

        // Rotate once through the normal, stateful client (mirrors the legitimate SPA).
        var firstRefresh = await _client.PostAsync("/api/auth/refresh", null);
        firstRefresh.EnsureSuccessStatusCode();

        // Replay the pre-rotation cookie, as a stolen/leaked token would be.
        using var replayRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        replayRequest.Headers.Add("Cookie", $"refreshToken={originalCookieValue}");
        var replayClient = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var replayResponse = await replayClient.SendAsync(replayRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, replayResponse.StatusCode);

        // The legitimately-rotated token (held by _client) must now also be revoked.
        var afterReuseResponse = await _client.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.Unauthorized, afterReuseResponse.StatusCode);
    }

    private static string ExtractSetCookie(HttpResponseMessage response) =>
        response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("refreshToken=", StringComparison.Ordinal));

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
