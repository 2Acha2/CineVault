using Asp.Versioning;
using CineVault.API.Models.Api;
using CineVault.API.DTOs;
using CineVault.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mapster;
using Microsoft.Extensions.Logging;

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
    public async Task<ActionResult<ApiResponseDto<List<ReviewDto>>>> GetReviews(
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
                .ProjectToType<ReviewDto>();  // Виконує мапінг прямо у запиті

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
            return Ok(ApiResponseDto<List<ReviewDto>>.Success(reviews, "Reviews retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching reviews.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<ReviewDto>>> GetReviewById(int id)
    {
        try
        {
            _logger.LogInformation("Fetching review with ID: {ReviewId}", id);

            var review = await _dbContext.Reviews
                .Include(r => r.Movie)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review is null)
            {
                _logger.LogWarning("Review with ID: {ReviewId} not found.", id);
                return NotFound(ApiResponseDto<ReviewDto>.Failure("Review not found", 404));
            }

            _logger.LogInformation("Fetched review with ID: {ReviewId} successfully.", id);
            return Ok(ApiResponseDto<ReviewDto>.Success(review.Adapt<ReviewDto>(), "Review retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching review with ID: {ReviewId}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ReviewDto>>> CreateReview([FromBody] ApiRequestDto<ReviewDto> request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseDto<ReviewDto>.Failure("Invalid data", 400));
        }

        try
        {
            _logger.LogInformation("Creating a new review for MovieId: {MovieId}, UserId: {UserId}", request.Data.MovieId, request.Data.UserId);

            var movieExists = await _dbContext.Movies.AnyAsync(m => m.Id == request.Data.MovieId);
            var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.Data.UserId);

            if (!movieExists || !userExists)
            {
                return BadRequest(ApiResponseDto<ReviewDto>.Failure("Invalid MovieId or UserId", 400));
            }

            if (request.Data.Rating is < 1 or > 10)
            {
                return BadRequest(ApiResponseDto<string>.Failure("Rating must be between 1 and 10", 400));
            }

            var review = request.Data.Adapt<Review>();
            review.CreatedAt = DateTime.UtcNow;

            _dbContext.Reviews.Add(review);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReviewById), new { id = review.Id },
                ApiResponseDto<ReviewDto>.Success(review.Adapt<ReviewDto>(), "Review created successfully", 201));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating a review.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<ReviewDto>>> UpdateReview(int id, [FromBody] ApiRequestDto<ReviewDto> request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseDto<ReviewDto>.Failure("Invalid data", 400));
        }

        try
        {
            _logger.LogInformation("Updating review with ID: {ReviewId}", id);

            var review = await _dbContext.Reviews.FindAsync(id);
            if (review is null)
            {
                return NotFound(ApiResponseDto<ReviewDto>.Failure("Review not found", 404));
            }

            var movieExists = await _dbContext.Movies.AnyAsync(m => m.Id == request.Data.MovieId);
            var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.Data.UserId);

            if (!movieExists || !userExists)
            {
                return BadRequest(ApiResponseDto<ReviewDto>.Failure("Invalid MovieId or UserId", 400));
            }

            var updateReview = request.Data with { Id = review.Id};
            updateReview.Adapt(review);

            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponseDto<ReviewDto>.Success(review.Adapt<ReviewDto>(), "Review updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating review with ID: {ReviewId}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<string>>> DeleteReview(int id)
    {
        try
        {
            _logger.LogInformation("Deleting review with ID: {ReviewId}", id);

            var review = await _dbContext.Reviews.FindAsync(id);
            if (review is null)
            {
                return NotFound(ApiResponseDto<string>.Failure("Review not found", 404));
            }

            _dbContext.Reviews.Remove(review);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponseDto<string>.Success($"Review with ID {id} deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting review with ID: {ReviewId}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}
