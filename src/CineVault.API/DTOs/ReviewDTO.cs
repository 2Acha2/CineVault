namespace CineVault.API.DTOs;

public record ReviewDto
{
    public required int Id { get; init; }
    public required int MovieId { get; init; }
    public required int UserId { get; init; }
    public required int Rating { get; init; }
    public string? Comment { get; init; }       // Можливість ставити відгук без коментаря
    public DateTime CreatedAt { get; init; }
}