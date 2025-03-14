using Asp.Versioning;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CineVault.API.Controllers.V1;
[ApiVersion(1)]
[Route("api/[controller]/[action]")]
public sealed class ReviewsController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger<ReviewsController> logger;

    public ReviewsController(CineVaultDbContext dbContext, ILogger<ReviewsController> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReviewResponse>>> GetReviews()
    {
        try
        {
            logger.LogInformation("Fetching all reviews from the database.");

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

            logger.LogInformation("Fetched {Count} reviews successfully.", reviews.Count);
            return Ok(reviews);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching reviews.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewResponse>> GetReviewById(int id)
    {
        try
        {
            logger.LogInformation("Fetching review with ID: {ReviewId}", id);

            var review = await this.dbContext.Reviews
                .Include(r => r.Movie)
                .Include(r => r.User)
                .FirstOrDefaultAsync(review => review.Id == id);

            if (review is null)
            {
                logger.LogWarning("Review with ID: {ReviewId} not found.", id);
                return NotFound();
            }

            logger.LogInformation("Fetched review with ID: {ReviewId} successfully.", id);

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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching review with ID: {ReviewId}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateReview(ReviewRequest request)
    {
        try
        {
            logger.LogInformation("Creating a new review for MovieId: {MovieId}, UserId: {UserId}", request.MovieId, request.UserId);

            var review = new Review
            {
                MovieId = request.MovieId,
                UserId = request.UserId,
                Rating = request.Rating,
                Comment = request.Comment
            };

            this.dbContext.Reviews.Add(review);
            await this.dbContext.SaveChangesAsync();

            logger.LogInformation("Review created successfully with MovieId: {MovieId}, UserId: {UserId}", request.MovieId, request.UserId);

            return Created();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating a review.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateReview(int id, ReviewRequest request)
    {
        try
        {
            logger.LogInformation("Updating review with ID: {ReviewId}", id);

            var review = await this.dbContext.Reviews.FindAsync(id);

            if (review is null)
            {
                logger.LogWarning("Review with ID: {ReviewId} not found.", id);
                return NotFound();
            }

            review.MovieId = request.MovieId;
            review.UserId = request.UserId;
            review.Rating = request.Rating;
            review.Comment = request.Comment;

            await this.dbContext.SaveChangesAsync();

            logger.LogInformation("Review with ID: {ReviewId} updated successfully.", id);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating review with ID: {ReviewId}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteReview(int id)
    {
        try
        {
            logger.LogInformation("Deleting review with ID: {ReviewId}", id);

            var review = await this.dbContext.Reviews.FindAsync(id);

            if (review is null)
            {
                logger.LogWarning("Review with ID: {ReviewId} not found.", id);
                return NotFound();
            }

            this.dbContext.Reviews.Remove(review);
            await this.dbContext.SaveChangesAsync();

            logger.LogInformation("Review with ID: {ReviewId} deleted successfully.", id);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting review with ID: {ReviewId}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}