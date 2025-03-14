using Asp.Versioning;
using CineVault.API.Models.Api;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetUsers()
    {
        _logger.LogInformation("Fetching all users.");

        var users = await _dbContext.Users
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email
            })
            .ToListAsync();

        return Ok(ApiResponse<List<UserResponse>>.Success(users, "Users retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById(int id)
    {
        _logger.LogInformation("Fetching user with ID: {UserId}", id);

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            _logger.LogWarning("User with ID: {UserId} not found.", id);
            return NotFound(ApiResponse<UserResponse>.Failure("User not found", 404));
        }

        var response = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };

        return Ok(ApiResponse<UserResponse>.Success(response, "User retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUser([FromBody] ApiRequest<UserRequest> request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<string>.Failure("Invalid user data", 400));
        }

        _logger.LogInformation("Creating a new user: {Username}", request.Data.Username);

        var user = new User
        {
            Username = request.Data.Username,
            Email = request.Data.Email,
            Password = request.Data.Password
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var response = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id },
            ApiResponse<UserResponse>.Success(response, "User created successfully", 201));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser(int id, [FromBody] ApiRequest<UserRequest> request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<string>.Failure("Invalid user data", 400));
        }

        _logger.LogInformation("Updating user with ID: {UserId}", id);

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            _logger.LogWarning("User with ID: {UserId} not found.", id);
            return NotFound(ApiResponse<UserResponse>.Failure("User not found", 404));
        }

        user.Username = request.Data.Username;
        user.Email = request.Data.Email;
        user.Password = request.Data.Password;

        await _dbContext.SaveChangesAsync();

        var response = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };

        return Ok(ApiResponse<UserResponse>.Success(response, "User updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteUser(int id)
    {
        _logger.LogInformation("Deleting user with ID: {UserId}", id);

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            _logger.LogWarning("User with ID: {UserId} not found.", id);
            return NotFound(ApiResponse<string>.Failure("User not found", 404));
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<string>.Success($"User with ID {id} deleted successfully"));
    }
}
