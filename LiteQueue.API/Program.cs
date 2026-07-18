using FluentValidation;
using FluentValidation.AspNetCore;
using LiteQueue.API.Validators;
using LiteQueue.Application.Services;
using LiteQueue.Background.Services;
using LiteQueue.Domain.Interfaces;
using LiteQueue.Infrastructure.Redis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateQueueRequestValidator>();

builder.Services.AddProblemDetails();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration.GetSection("LiteQueue:Redis:ConnectionString").Value ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(connectionString);
});

builder.Services.AddScoped<IQueueRepository, RedisQueueRepository>();
builder.Services.AddScoped<QueueService>();
builder.Services.AddHostedService<MessageCleaner>();

builder.Services.AddHealthChecks()
    .AddCheck<RedisHealthCheck>("redis", tags: new[] { "ready" });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.MapHealthChecks("/api/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/api/health/ready", new() { Predicate = check => check.Tags.Contains("ready") });
app.MapHealthChecks("/api/health");

app.MapGet("/", () => "LiteQueue API is running.");

app.Run();

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;

    public RedisHealthCheck(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _redis.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy("Redis is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis is unreachable.", ex);
        }
    }
}
