namespace CineVault.API.DTOs;

public record ApiRequestDto<T>(T Data, Dictionary<string, string> Meta)
{
    public ApiRequestDto(T data) : this(data, new Dictionary<string, string>()) { }
}
