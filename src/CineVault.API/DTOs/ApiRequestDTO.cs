using System.Text.Json.Serialization;

namespace CineVault.API.DTOs;

public record ApiRequestDto<T>
{
    public T Data { get; init; }
    public Dictionary<string, string> Meta { get; init; } = new();

    [JsonConstructor]
    public ApiRequestDto(T data, Dictionary<string, string> meta)
    {
        Data = data;
        Meta = meta ?? new Dictionary<string, string>();
    }

    public ApiRequestDto(T data) : this(data, new Dictionary<string, string>()) { }

    public ApiRequestDto() { }
}
