namespace CineVault.API.DTOs;

public record UserDto
{
    public int? Id { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public string? Password { get; init; }
    public DateTime CreatedAt { get; init; }
}