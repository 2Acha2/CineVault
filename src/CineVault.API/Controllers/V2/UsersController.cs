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
public sealed class UsersController : ControllerBase
{
    private readonly CineVaultDbContext _dbContext;
    private readonly ILogger<UsersController> _logger;

    public UsersController(CineVaultDbContext dbContext, ILogger<UsersController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<UserDto>>>> GetUsers()
    {
        _logger.LogInformation("Fetching all users.");

        var users = await _dbContext.Users
            .ProjectToType<UserDto>()
            .ToListAsync();

        return Ok(ApiResponseDto<List<UserDto>>.Success(users, "Users retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> GetUserById(int id)
    {
        _logger.LogInformation("Fetching user with ID: {UserId}", id);

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            _logger.LogWarning("User with ID: {UserId} not found.", id);
            return NotFound(ApiResponseDto<UserDto>.Failure("User not found", 404));
        }

        return Ok(ApiResponseDto<UserDto>.Success(user.Adapt<UserDto>(), "User retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> CreateUser([FromBody] ApiRequestDto<UserDto> request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseDto<string>.Failure("Invalid user data", 400));
        }

        _logger.LogInformation("Creating a new user: {Username}", request.Data.Username);

        var user = request.Data.Adapt<User>();  

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id },
            ApiResponseDto<UserDto>.Success(user.Adapt<UserDto>(), "User created successfully", 201));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> UpdateUser(int id, [FromBody] ApiRequestDto<UserDto> request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseDto<string>.Failure("Invalid user data", 400));
        }

        _logger.LogInformation("Updating user with ID: {UserId}", id);

        var user = await _dbContext.Users.FindAsync(id);
        if (user is null)
        {
            _logger.LogWarning("User with ID: {UserId} not found.", id);
            return NotFound(ApiResponseDto<UserDto>.Failure("User not found", 404));
        }

        var upadteUser = request.Data with { Id = user.Id };
        upadteUser.Adapt(user);

        await _dbContext.SaveChangesAsync();
        return Ok(ApiResponseDto<UserDto>.Success(user.Adapt<UserDto>(), "User updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<string>>> DeleteUser(int id)
    {
        _logger.LogInformation("Deleting user with ID: {UserId}", id);

        var user = await _dbContext.Users.FindAsync(id);
        if (user is null)
        {
            _logger.LogWarning("User with ID: {UserId} not found.", id);
            return NotFound(ApiResponseDto<string>.Failure("User not found", 404));
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<string>.Success($"User with ID {id} deleted successfully"));
    }
}
