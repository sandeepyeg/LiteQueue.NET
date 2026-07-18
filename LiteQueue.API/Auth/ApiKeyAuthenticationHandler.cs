using System.Security.Claims;
using System.Text.Encodings.Web;
using LiteQueue.API.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace LiteQueue.API.Auth;

public static class ApiKeyDefaults
{
    public const string AuthenticationScheme = "ApiKey";
}

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IOptions<LiteQueueApiOptions> _apiOptions;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<LiteQueueApiOptions> apiOptions)
        : base(options, logger, encoder)
    {
        _apiOptions = apiOptions;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader))
            return Task.FromResult(AuthenticateResult.Fail("Missing X-Api-Key header"));

        var apiKey = apiKeyHeader.ToString();
        var options = _apiOptions.Value;

        if (string.IsNullOrWhiteSpace(options.ApiKey) && string.IsNullOrWhiteSpace(options.AdminApiKey))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim>();
        var isAuthenticated = false;

        if (!string.IsNullOrWhiteSpace(options.AdminApiKey) && apiKey == options.AdminApiKey)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            claims.Add(new Claim("scope", "admin"));
            claims.Add(new Claim("scope", "queues"));
            claims.Add(new Claim("scope", "topics"));
            isAuthenticated = true;
        }
        else if (!string.IsNullOrWhiteSpace(options.ApiKey) && apiKey == options.ApiKey)
        {
            claims.Add(new Claim(ClaimTypes.Role, "User"));
            claims.Add(new Claim("scope", "queues"));
            claims.Add(new Claim("scope", "topics"));
            isAuthenticated = true;
        }

        if (!isAuthenticated)
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
