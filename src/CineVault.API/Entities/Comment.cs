namespace CineVault.API.Entities;

public sealed class Comment
{
    public int Id { get; set; }
    public required int ReviewId { get; set; }
    public required int UserId { get; set; }
    public string? Content { get; set; }
    public required int Rating { get; set; }

    public Review? Review { get; set; }
    public User? User { get; set; }
    public ICollection<Like> Likes { get; set; } = [];

}
