using Microsoft.Extensions.DependencyInjection;

namespace LiteQueue.Worker;

public static class ServiceCollectionExtensions
{
    internal static readonly List<Type> RegisteredHandlerTypes = new();

    public static IServiceCollection AddLiteQueueWorker(
        this IServiceCollection services,
        Action<LiteQueueWorkerOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddHttpClient("LiteQueueWorker", (sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<LiteQueueWorkerOptions>>().Value;
            if (options.BaseAddress != null)
                client.BaseAddress = options.BaseAddress;
        });

        services.AddSingleton(RegisteredHandlerTypes);
        services.AddHostedService<LiteQueueWorker>();

        return services;
    }

    public static IServiceCollection AddLiteQueueHandler<TMessage, THandler>(this IServiceCollection services)
        where THandler : class, ILiteQueueHandler<TMessage>
    {
        services.AddScoped<ILiteQueueHandler<TMessage>, THandler>();
        RegisteredHandlerTypes.Add(typeof(THandler));
        return services;
    }
}
