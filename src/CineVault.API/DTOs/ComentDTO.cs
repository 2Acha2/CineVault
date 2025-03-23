namespace CineVault.API.DTOs;

public record CommentDto
{
    public int? Id { get; init; }
    public required int ReviewId { get; init; }
    public required int UserId { get; init; }
    public required int Rating { get; init; }
    public  string? Content { get; init; }
    
}
