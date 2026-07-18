using System.Collections.Concurrent;
using System.Net;

namespace LiteQueue.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new();

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var apiKey = ResolveApiKey(context);
        var key = apiKey ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        var windowStart = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 60;

        var entry = _entries.GetOrAdd(key, _ => new RateLimitEntry(windowStart, 0));

        lock (entry)
        {
            if (entry.WindowStart != windowStart)
            {
                entry.WindowStart = windowStart;
                entry.Count = 0;
            }

            entry.Count++;

            if (entry.Count > 1000)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers["Retry-After"] = "60";
                return;
            }
        }

        await _next(context);
    }

    private static string? ResolveApiKey(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
            return apiKey.ToString();
        return null;
    }

    private class RateLimitEntry
    {
        public long WindowStart;
        public int Count;

        public RateLimitEntry(long windowStart, int count)
        {
            WindowStart = windowStart;
            Count = count;
        }
    }
}
