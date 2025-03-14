using Asp.Versioning;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CineVault.API.Controllers.V2;

[ApiVersion(2)]
[Route("api/[controller]/[action]")]
[ApiController]
public sealed class ReviewsController : ControllerBase
{
    private readonly CineVaultDbContext _dbContext;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(CineVaultDbContext dbContext, ILogger<ReviewsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReviewResponse>>> GetReviews(
        string? orderBy = "createdAt",
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Fetching reviews with sorting: {OrderBy}, page: {Page}, pageSize: {PageSize}", orderBy, page, pageSize);

            var query = _dbContext.Reviews
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
                });

            query = orderBy switch
            {
                "rating" => query.OrderBy(r => r.Rating),
                "rating_desc" => query.OrderByDescending(r => r.Rating),
                _ => query.OrderByDescending(r => r.CreatedAt)
            };

            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Fetched {Count} reviews successfully.", reviews.Count);
            return Ok(reviews);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching reviews.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewResponse>> GetReviewById(int id)
    {
        try
        {
            _logger.LogInformation("Fetching review with ID: {ReviewId}", id);

            var review = await _dbContext.Reviews
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
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review is null)
            {
                _logger.LogWarning("Review with ID: {ReviewId} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Fetched review with ID: {ReviewId} successfully.", id);
            return Ok(review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching review with ID: {ReviewId}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateReview([FromBody] ReviewRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("Creating a new review for MovieId: {MovieId}, UserId: {UserId}", request.MovieId, request.UserId);

            var movieExists = await _dbContext.Movies.AnyAsync(m => m.Id == request.MovieId);
            var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.UserId);

            if (!movieExists || !userExists)
            {
                return BadRequest("Invalid MovieId or UserId.");
            }

            var review = new Review
            {
                MovieId = request.MovieId,
                UserId = request.UserId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Reviews.Add(review);
            await _dbContext.SaveChangesAsync();

            return CreatedAtRoute(nameof(GetReviewById), new { id = review.Id }, review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating a review.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateReview(int id, [FromBody] ReviewRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("Updating review with ID: {ReviewId}", id);

            var review = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == id);
            if (review is null)
            {
                return NotFound();
            }

            var movieExists = await _dbContext.Movies.AnyAsync(m => m.Id == request.MovieId);
            var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.UserId);

            if (!movieExists || !userExists)
            {
                return BadRequest("Invalid MovieId or UserId.");
            }

            review.MovieId = request.MovieId;
            review.UserId = request.UserId;
            review.Rating = request.Rating;
            review.Comment = request.Comment;

            await _dbContext.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating review with ID: {ReviewId}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteReview(int id)
    {
        try
        {
            _logger.LogInformation("Deleting review with ID: {ReviewId}", id);

            var review = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == id);
            if (review is null)
            {
                return NotFound();
            }

            _dbContext.Reviews.Remove(review);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting review with ID: {ReviewId}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}
