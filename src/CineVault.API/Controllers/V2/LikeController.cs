using Asp.Versioning;
using CineVault.API.Models.Api;
using CineVault.API.DTOs;
using CineVault.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mapster;

// 5.Підтримка лайків для відгуків-коментарів з оцінкою
namespace CineVault.API.Controllers.V2;

[ApiVersion(2)]
[Route("api/[controller]/[action]")]
[ApiController]
public sealed class LikesController : ControllerBase
{
    private readonly CineVaultDbContext _dbContext;

    public LikesController(CineVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<int>>> GetLikesForReview(int reviewId)
    {
        var likeCount = await _dbContext.Likes.CountAsync(l => l.ReviewId == reviewId);
        return Ok(ApiResponseDto<int>.Success(likeCount, "Like count retrieved successfully"));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<int>>> GetLikesForComment(int commentId)
    {
        var likeCount = await _dbContext.Likes.CountAsync(l => l.CommentId == commentId);
        return Ok(ApiResponseDto<int>.Success(likeCount, "Like count retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<string>>> Like([FromBody] ApiRequestDto<LikeDto> request)
    {
        if (request.Data is null)
        {
            return BadRequest(ApiResponseDto<string>.Failure("Request data is missing", 400));
        }

        if (request.Data.ReviewId is not null && request.Data.CommentId is not null)
        {
            return BadRequest(ApiResponseDto<string>.Failure("Like must be for either a review or a comment, not both", 400));
        }

        // Додати можливість лайкати коментарі тільки зареєстрованим користувачам
        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.Data.UserId);
        if (!userExists)
        {
            return BadRequest(ApiResponseDto<string>.Failure("Only registered users can like reviews and comments", 400));
        }

        var existingLike = await _dbContext.Likes.FirstOrDefaultAsync(l =>
            l.UserId == request.Data.UserId &&
            (l.ReviewId == request.Data.ReviewId || l.CommentId == request.Data.CommentId));

        if (existingLike is not null)
        {
            return BadRequest(ApiResponseDto<string>.Failure("User has already liked this item", 400));
        }

        var like = request.Data.Adapt<Like>();

        await _dbContext.Likes.AddAsync(like);
        await _dbContext.SaveChangesAsync();

        return Created(string.Empty, ApiResponseDto<string>.Success("Like added successfully", "201"));
    }

    [HttpDelete]
    public async Task<ActionResult<ApiResponseDto<string>>> Unlike([FromBody] ApiRequestDto<LikeDto> request)
    {
        var like = await _dbContext.Likes.FirstOrDefaultAsync(l =>
            l.UserId == request.Data.UserId &&
            (l.ReviewId == request.Data.ReviewId || l.CommentId == request.Data.CommentId));

        if (like is null)
        {
            return NotFound(ApiResponseDto<string>.Failure("Like not found", 404));
        }

        _dbContext.Likes.Remove(like);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<string>.Success("Like removed successfully"));
    }
}
