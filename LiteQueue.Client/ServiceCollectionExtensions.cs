using Microsoft.Extensions.DependencyInjection;

namespace LiteQueue.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLiteQueueClient(
        this IServiceCollection services,
        Action<LiteQueueClientOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddHttpClient<LiteQueueClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<LiteQueueClientOptions>>().Value;
            if (options.BaseAddress != null)
                client.BaseAddress = options.BaseAddress;
            if (options.DefaultTimeout > TimeSpan.Zero)
                client.Timeout = options.DefaultTimeout;
        });

        return services;
    }
}
