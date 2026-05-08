using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AuthApi.IntegrationTests;

public class AuthenticationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthenticationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_ReturnsTokens_With_TenantClaim()
    {
        var login = new { Email = "usera@example.com", Password = "P@ssw0rd1" };
        var resp = await _client.PostAsJsonAsync("/api/auth/login", login);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var access = doc.RootElement.GetProperty("accessToken").GetString();
        var refresh = doc.RootElement.GetProperty("refreshToken").GetString();
        Assert.False(string.IsNullOrEmpty(access));
        Assert.False(string.IsNullOrEmpty(refresh));

        // ensure JWT contains tenantId claim
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(access);
        var tenantClaim = token.Claims.FirstOrDefault(c => c.Type == "tenantId")?.Value;
        Assert.False(string.IsNullOrEmpty(tenantClaim));
    }

    [Fact]
    public async Task Refresh_Rotates_RefreshToken()
    {
        // login to get access + refresh
        var login = new { Email = "usera@example.com", Password = "P@ssw0rd1" };
        var resp = await _client.PostAsJsonAsync("/api/auth/login", login);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var access = doc.RootElement.GetProperty("accessToken").GetString();
        var refresh = doc.RootElement.GetProperty("refreshToken").GetString();

        var refreshReq = new { AccessToken = access, RefreshToken = refresh };
        var r = await _client.PostAsJsonAsync("/api/auth/refresh", refreshReq);
        r.EnsureSuccessStatusCode();
        var j2 = await r.Content.ReadAsStringAsync();
        using var doc2 = JsonDocument.Parse(j2);
        var newRefresh = doc2.RootElement.GetProperty("refreshToken").GetString();
        Assert.False(string.IsNullOrEmpty(newRefresh));
        Assert.NotEqual(refresh, newRefresh);

        // second rotation with old refresh should fail
        var second = await _client.PostAsJsonAsync("/api/auth/refresh", new { AccessToken = access, RefreshToken = refresh });
        Assert.Equal(HttpStatusCode.Unauthorized, second.StatusCode);
    }

    [Fact]
    public async Task TenantIsolation_Prevents_Using_OtherTenantRefreshToken()
    {
        // login as userA
        var login = new { Email = "usera@example.com", Password = "P@ssw0rd1" };
        var resp = await _client.PostAsJsonAsync("/api/auth/login", login);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var access = doc.RootElement.GetProperty("accessToken").GetString();

        // use userB's refresh token with userA access token -> should be unauthorized
        var userBId = _factory.SeededUserIds["userB"];
        var otherRefresh = _factory.SeededUserRefreshTokens[userBId];

        var refreshReq = new { AccessToken = access, RefreshToken = otherRefresh };
        var r = await _client.PostAsJsonAsync("/api/auth/refresh", refreshReq);
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }
}