namespace CineVault.API.Entities;

public sealed class Review
{
    public int Id { get; set; }
    public required int MovieId { get; set; }
    public required int UserId { get; set; }    // Ставити відгук може лише зареєстрований користувач
    public required int Rating { get; set; }
    public string? Comment { get; set; }    // Можливість ставити відгук без коментаря
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Movie? Movie { get; set; }
    public User? User { get; set; }
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Like> Likes { get; set; } = [];
}