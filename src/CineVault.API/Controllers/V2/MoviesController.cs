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
public sealed class MoviesController : ControllerBase
{
    private readonly CineVaultDbContext _dbContext;

    public MoviesController(CineVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<MovieDto>>>> GetMovies()
    {
        var movies = await _dbContext.Movies
            .Include(m => m.Reviews)
            .ProjectToType<MovieDto>()
            .ToListAsync();

        return Ok(ApiResponseDto<List<MovieDto>>.Success(movies, "Movies retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<MovieDto>>> GetMovieById(int id)
    {
        var movie = await _dbContext.Movies
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie is null)
        {
            return NotFound(ApiResponseDto<MovieDto>.Failure("Movie not found", 404));
        }

        return Ok(ApiResponseDto<MovieDto>.Success(movie.Adapt<MovieDto>(), "Movie retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<MovieDto>>> CreateMovie([FromBody] ApiRequestDto<MovieDto> request)
    {
        var movie = request.Data.Adapt<Movie>(); 

        await _dbContext.Movies.AddAsync(movie);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMovieById), new { id = movie.Id },
            ApiResponseDto<MovieDto>.Success(movie.Adapt<MovieDto>(), "Movie created successfully", 201));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<int>>> BulkCreateMovies([FromBody] ApiRequestDto<List<MovieDto>> request)
    {
        if (request.Data == null || !request.Data.Any())
        {
            return BadRequest(ApiResponseDto<int>.Failure("Movie list cannot be empty", 400));
        }

        var movies = request.Data.Adapt<List<Movie>>(); // Mapster: DTO → Entity

        await _dbContext.Movies.AddRangeAsync(movies);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<int>.Success(movies.Count, $"{movies.Count} movies added successfully"));
    }


    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<MovieDto>>> UpdateMovie(int id, [FromBody] ApiRequestDto<MovieDto> request)
    {
        var movie = await _dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            return NotFound(ApiResponseDto<MovieDto>.Failure("Movie not found", 404));
        }

        var updatedMovie = request.Data with { Id = movie.Id };

        updatedMovie.Adapt(movie);

        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<MovieDto>.Success(movie.Adapt<MovieDto>(), "Movie updated successfully"));
    }


    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<string>>> DeleteMovie(int id)
    {
        var movie = await _dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            return NotFound(ApiResponseDto<string>.Failure("Movie not found", 404));
        }

        _dbContext.Movies.Remove(movie);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<string>.Success($"Movie with ID {id} deleted successfully"));
    }
}
