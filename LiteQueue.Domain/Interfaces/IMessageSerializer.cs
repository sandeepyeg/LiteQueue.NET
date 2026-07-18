using System.Text.Json;

namespace LiteQueue.Domain.Interfaces;

public interface IMessageSerializer
{
    string Serialize<T>(T message);
    T? Deserialize<T>(string json);
}

public class SystemTextJsonMessageSerializer : IMessageSerializer
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Serialize<T>(T message) => JsonSerializer.Serialize(message, _options);
    public T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _options);
}
