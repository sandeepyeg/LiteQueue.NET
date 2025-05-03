using LiteQueue.Domain.Interfaces;
using LiteQueue.Infrastructure.Redis;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Redis + QueueRepository before app.Build()
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379"));

builder.Services.AddScoped<IQueueRepository, RedisQueueRepository>();
builder.Services.AddScoped<QueueService>();
builder.Services.AddHostedService<MessageCleaner>();


var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "LiteQueue API is running.");

app.Run();