using Asp.Versioning;
using CineVault.API.Models.Api;
using CineVault.API.DTOs;
using CineVault.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace CineVault.API.Controllers.V2;

[ApiVersion(2)]
[Route("api/[controller]/[action]")]
[ApiController]
public sealed class CommentsController : ControllerBase
{
    private readonly CineVaultDbContext _dbContext;

    public CommentsController(CineVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Реалізувати CRUD для коментарів до відгуків
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<CommentDto>>>> GetAllComments()
    {
        var comments = await _dbContext.Comments
            .Include(c => c.Review)
            .ThenInclude(r => r.Movie)
            .Include(c => c.User)
            .ProjectToType<CommentDto>()
            .ToListAsync();

        return Ok(ApiResponseDto<List<CommentDto>>.Success(comments));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<string>>> CreateComment([FromBody] ApiRequestDto<CommentDto> request)
    {
        if (request.Data is null)
        {
            return BadRequest(ApiResponseDto<string>.Failure("Request data is missing", 400));
        }

        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.Data.UserId);
        var reviewExists = await _dbContext.Reviews.AnyAsync(r => r.Id == request.Data.ReviewId);

        if (!userExists || !reviewExists)
        {
            return BadRequest(ApiResponseDto<string>.Failure("Invalid user or review", 400));
        }

        if (request.Data.Rating is < 1 or > 10)
        {
            return BadRequest(ApiResponseDto<string>.Failure("Rating must be between 1 and 10", 400));
        }

        var comment = request.Data.Adapt<Comment>();

        await _dbContext.Comments.AddAsync(comment);
        await _dbContext.SaveChangesAsync();

        // 8. Доробити всі методи по створенню
        var response = new { comment.Id };
        return CreatedAtAction(nameof(GetAllComments), null,
            ApiResponseDto<object>.Success(response, "Comment created successfully", 201));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<string>>> UpdateComment(int id, [FromBody] ApiRequestDto<CommentDto> request)
    {
        if (request.Data is null)
        {
            return BadRequest(ApiResponseDto<string>.Failure("Request data is missing", 400));
        }

        var comment = await _dbContext.Comments.FindAsync(id);
        if (comment is null)
        {
            return NotFound(ApiResponseDto<string>.Failure("Comment not found", 404));
        }

        if (request.Data.Rating is < 1 or > 10)
        {
            return BadRequest(ApiResponseDto<string>.Failure("Rating must be between 1 and 10", 400));
        }

        request.Data.Adapt(comment);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<string>.Success("Comment updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<string>>> DeleteComment(int id)
    {
        var comment = await _dbContext.Comments.FindAsync(id);
        if (comment is null)
        {
            return NotFound(ApiResponseDto<string>.Failure("Comment not found", 404));
        }

        _dbContext.Comments.Remove(comment);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<string>.Success("Comment deleted successfully"));
    }
}
