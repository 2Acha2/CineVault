namespace CineVault.API.DTOs;

public record LikeDto
{
    public required int Id { get; init; }
    public required int UserId { get; init; }
    public int? ReviewId { get; init; }
    public int? CommentId { get; init; }
}
