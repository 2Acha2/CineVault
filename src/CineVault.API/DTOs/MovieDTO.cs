namespace CineVault.API.DTOs;

public record MovieDto
{
    public int? Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public string? Genre { get; init; }
    public string? Director { get; init; }
}