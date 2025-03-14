using Asp.Versioning;
using CineVault.API.Models.Api;
using CineVault.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;

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
    public async Task<ActionResult<ApiResponse<List<MovieResponse>>>> GetMovies()
    {
        var movies = await _dbContext.Movies
            .Include(m => m.Reviews)
            .Select(m => new MovieResponse
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseDate = m.ReleaseDate,
                Genre = m.Genre,
                Director = m.Director,
                AverageRating = m.Reviews.Count != 0
                    ? m.Reviews.Average(r => r.Rating)
                    : 0,
                ReviewCount = m.Reviews.Count
            })
            .ToListAsync();

        return Ok(ApiResponse<List<MovieResponse>>.Success(movies, "Movies retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> GetMovieById(int id)
    {
        var movie = await _dbContext.Movies
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie is null)
        {
            return NotFound(ApiResponse<MovieResponse>.Failure("Movie not found", 404));
        }

        var response = new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseDate = movie.ReleaseDate,
            Genre = movie.Genre,
            Director = movie.Director,
            AverageRating = movie.Reviews.Count != 0
                ? movie.Reviews.Average(r => r.Rating)
                : 0,
            ReviewCount = movie.Reviews.Count
        };

        return Ok(ApiResponse<MovieResponse>.Success(response, "Movie retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> CreateMovie([FromBody] ApiRequest<MovieRequest> request)
    {
        var movie = new Movie
        {
            Title = request.Data.Title,
            Description = request.Data.Description,
            ReleaseDate = request.Data.ReleaseDate,
            Genre = request.Data.Genre,
            Director = request.Data.Director
        };

        await _dbContext.Movies.AddAsync(movie);
        await _dbContext.SaveChangesAsync();

        var response = new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseDate = movie.ReleaseDate,
            Genre = movie.Genre,
            Director = movie.Director,
            AverageRating = 0,
            ReviewCount = 0
        };

        return Created("", ApiResponse<MovieResponse>.Success(response, "Movie created successfully", 201));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> UpdateMovie(int id, [FromBody] ApiRequest<MovieRequest> request)
    {
        var movie = await _dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            return NotFound(ApiResponse<MovieResponse>.Failure("Movie not found", 404));
        }

        movie.Title = request.Data.Title;
        movie.Description = request.Data.Description;
        movie.ReleaseDate = request.Data.ReleaseDate;
        movie.Genre = request.Data.Genre;
        movie.Director = request.Data.Director;

        await _dbContext.SaveChangesAsync();

        var response = new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseDate = movie.ReleaseDate,
            Genre = movie.Genre,
            Director = movie.Director,
            AverageRating = 0,
            ReviewCount = 0
        };

        return Ok(ApiResponse<MovieResponse>.Success(response, "Movie updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteMovie(int id)
    {
        var movie = await _dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            return NotFound(ApiResponse<string>.Failure("Movie not found", 404));
        }

        _dbContext.Movies.Remove(movie);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<string>.Success($"Movie with ID {id} deleted successfully"));
    }
}
