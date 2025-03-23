namespace CineVault.API.Entities;

public class Like
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public int? ReviewId { get; set; }
    public int? CommentId { get; set; }

    public User? User { get; set; }
    public Review? Review { get; set; }
    public Comment? Comment { get; set; }
}
