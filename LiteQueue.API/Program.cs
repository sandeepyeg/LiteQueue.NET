using FluentValidation;
using FluentValidation.AspNetCore;
using LiteQueue.API;
using LiteQueue.API.Auth;
using LiteQueue.API.Validators;
using LiteQueue.Application.Services;
using LiteQueue.Background.Services;
using LiteQueue.Domain.Interfaces;
using LiteQueue.Infrastructure.Redis;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<LiteQueue.API.Options.LiteQueueApiOptions>(
    builder.Configuration.GetSection("LiteQueue:Api"));

builder.Services.AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);

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
builder.Services.AddScoped<ISchedulerRepository, RedisSchedulerRepository>();
builder.Services.AddScoped<ITopicRepository, RedisTopicRepository>();
builder.Services.AddScoped<QueueService>();
builder.Services.AddScoped<QueueMonitoringService>();
builder.Services.AddScoped<SchedulingService>();
builder.Services.AddScoped<TopicService>();
builder.Services.AddScoped<MessageDeduplicationService>();

builder.Services.AddHostedService<MessageCleaner>();
builder.Services.AddHostedService<VisibilityTimeoutWorker>();
builder.Services.AddHostedService<DelayedMessagePromotionWorker>();
builder.Services.AddHostedService<RecurringJobScheduler>();
builder.Services.AddHostedService<DeadLetterRetentionWorker>();
builder.Services.AddHostedService<ExpiredMessageCleaner>();
builder.Services.AddHostedService<QueueStatisticsWorker>();
builder.Services.AddHostedService<OrphanedLeaseRecoveryWorker>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks()
    .AddCheck<RedisHealthCheck>("redis", tags: new[] { "ready" });

builder.Services.AddLiteQueueTelemetry();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/api/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/api/health/ready", new() { Predicate = check => check.Tags.Contains("ready") });
app.MapHealthChecks("/api/health");

app.UseOpenTelemetryPrometheusScrapingEndpoint();
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
