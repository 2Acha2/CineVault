using Asp.Versioning;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Controllers.V1;
[ApiVersion(1)]
[Route("api/[controller]/[action]")]
public sealed class ReviewsController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;

    public ReviewsController(CineVaultDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReviewResponse>>> GetReviews()
    {
        var reviews = await this.dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                MovieId = r.MovieId,
                MovieTitle = r.Movie!.Title,
                UserId = r.UserId,
                Username = r.User!.Username,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewResponse>> GetReviewById(int id)
    {
        var review = await this.dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .FirstOrDefaultAsync(review => review.Id == id);

        if (review is null)
        {
            return NotFound();
        }

        var response = new ReviewResponse
        {
            Id = review.Id,
            MovieId = review.MovieId,
            MovieTitle = review.Movie!.Title,
            UserId = review.UserId,
            Username = review.User!.Username,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> CreateReview(ReviewRequest request)
    {
        var review = new Review
        {
            MovieId = request.MovieId,
            UserId = request.UserId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        this.dbContext.Reviews.Add(review);
        await this.dbContext.SaveChangesAsync();

        return Created();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateReview(int id, ReviewRequest request)
    {
        var review = await this.dbContext.Reviews.FindAsync(id);

        if (review is null)
        {
            return NotFound();
        }

        review.MovieId = request.MovieId;
        review.UserId = request.UserId;
        review.Rating = request.Rating;
        review.Comment = request.Comment;

        await this.dbContext.SaveChangesAsync();

        return Ok();
    }

    //[HttpDelete("{id}")]
    //public async Task<ActionResult> DeleteReview(int id)
    //{
    //    var review = await this.dbContext.Reviews.FindAsync(id);

    //    if (review is null)
    //    {
    //        return NotFound();
    //    }

    //    this.dbContext.Reviews.Remove(review);
    //    await this.dbContext.SaveChangesAsync();

    //    return Ok();
    //}
}