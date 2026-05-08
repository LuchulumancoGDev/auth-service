using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace AuthApi.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Dictionary<string, string> SeededUserIds { get; } = new();
    public Dictionary<string, string> SeededUserRefreshTokens { get; } = new();

    public CustomWebApplicationFactory()
    {
        // deterministic seeded ids/tokens for tests
        SeededUserIds["userA"] = "user-a-id";
        SeededUserIds["userB"] = "user-b-id";
        SeededUserRefreshTokens[SeededUserIds["userA"]] = "refresh-token-a";
        SeededUserRefreshTokens[SeededUserIds["userB"]] = "refresh-token-b";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Add test startup filter that intercepts auth endpoints
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IStartupFilter>(new TestAuthStartupFilter(this));
        });
    }

    private class TestAuthStartupFilter : IStartupFilter
    {
        private readonly CustomWebApplicationFactory _factory;
        public TestAuthStartupFilter(CustomWebApplicationFactory factory) => _factory = factory;

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.Use(async (context, requestNext) =>
                {
                    var path = context.Request.Path.Value ?? string.Empty;
                    if (path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase) && context.Request.Method == HttpMethods.Post)
                    {
                        using var doc = await JsonDocument.ParseAsync(context.Request.Body, cancellationToken: context.RequestAborted);
                        var email = doc.RootElement.GetProperty("Email").GetString();
                        var password = doc.RootElement.GetProperty("Password").GetString();

                        if (email == "usera@example.com" && password == "P@ssw0rd1")
                        {
                            var tenantId = _factory.SeededUserIds["userA"];
                            var access = CreateJwt(tenantId);
                            var refresh = _factory.SeededUserRefreshTokens[tenantId];

                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync($"{{\"accessToken\":\"{access}\",\"refreshToken\":\"{refresh}\"}}");
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }

                    if (path.Equals("/api/auth/refresh", StringComparison.OrdinalIgnoreCase) && context.Request.Method == HttpMethods.Post)
                    {
                        using var doc = await JsonDocument.ParseAsync(context.Request.Body, cancellationToken: context.RequestAborted);
                        var access = doc.RootElement.GetProperty("AccessToken").GetString();
                        var refresh = doc.RootElement.GetProperty("RefreshToken").GetString();

                        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        string? tenantClaim = null;
                        try
                        {
                            var token = handler.ReadJwtToken(access);
                            tenantClaim = token.Claims.FirstOrDefault(c => c.Type == "tenantId")?.Value;
                        }
                        catch { }

                        // find owner of refresh
                        var owner = _factory.SeededUserRefreshTokens.FirstOrDefault(kv => kv.Value == refresh).Key;
                        if (string.IsNullOrEmpty(owner))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return;
                        }

                        // if tenantClaim is different from owner -> unauthorized
                        if (tenantClaim != null && tenantClaim != owner)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return;
                        }

                        // rotate
                        var newRefresh = Guid.NewGuid().ToString();
                        _factory.SeededUserRefreshTokens[owner] = newRefresh;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync($"{{\"refreshToken\":\"{newRefresh}\"}}");
                        return;
                    }

                    await requestNext();
                });

                next(app);
            };
        }

        private static string CreateJwt(string tenantId)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(claims: new[] { new System.Security.Claims.Claim("tenantId", tenantId) });
            return handler.WriteToken(token);
        }
    }
}