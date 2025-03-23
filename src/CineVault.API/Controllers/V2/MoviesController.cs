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

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<MovieDto>>>> SearchMovies(
    string? title = null,
    string? genre = null,
    string? director = null,
    int? releaseYear = null,
    double? averageRating = null,
    string? orderBy = "releaseDate_desc",
    int page = 1,
    int pageSize = 10)
    {
        var query = _dbContext.Movies
            .Include(m => m.Reviews)
            .AsQueryable();

        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(m => m.Title.Contains(title));
        }
        if (!string.IsNullOrEmpty(genre))
        {
            query = query.Where(m => m.Genre.Contains(genre));
        }
        if (!string.IsNullOrEmpty(director))
        {
            query = query.Where(m => m.Director.Contains(director));
        }


        if (releaseYear.HasValue)
        {
            query = query.Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == releaseYear.Value);
        }


        if (averageRating.HasValue)
        {
            query = query.Where(m => m.Reviews.Any() && m.Reviews.Average(r => r.Rating) >= averageRating.Value);
        }

       
        query = orderBy switch
        {
            "title" => query.OrderBy(m => m.Title),
            "title_desc" => query.OrderByDescending(m => m.Title),
            "releaseDate" => query.OrderBy(m => m.ReleaseDate),
            "rating" => query.OrderBy(m => m.Reviews.Average(r => r.Rating)),
            "rating_desc" => query.OrderByDescending(m => m.Reviews.Average(r => r.Rating)),
            _ => query.OrderByDescending(m => m.ReleaseDate) // releaseDate_desc за замовчуванням
        };

    
        var totalMovies = await query.CountAsync();
        var movies = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<MovieDto>()
            .ToListAsync();

        return Ok(ApiResponseDto<List<MovieDto>>.Success(movies, $"Found {totalMovies} movies"));
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

        var movies = request.Data.Adapt<List<Movie>>();

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

    [HttpDelete("DeleteMultiple")]
    public async Task<ActionResult<ApiResponseDto<string>>> DeleteMovies([FromBody] ApiRequestDto<List<int>> request)
    {
        if (request.Data is null || !request.Data.Any())
        {
            return BadRequest(ApiResponseDto<string>.Failure("No movie IDs provided", 400));
        }

        var movies = await _dbContext.Movies
            .Where(m => request.Data.Contains(m.Id))
            .Include(m => m.Reviews)
            .ToListAsync();

        if (!movies.Any())
        {
            return NotFound(ApiResponseDto<string>.Failure("No matching movies found", 404));
        }

        var moviesWithReviews = movies.Where(m => m.Reviews.Any()).ToList();
        var moviesToDelete = movies.Except(moviesWithReviews).ToList();

        if (moviesToDelete.Any())
        {
            _dbContext.Movies.RemoveRange(moviesToDelete);
            await _dbContext.SaveChangesAsync();
        }

        return Ok(ApiResponseDto<string>.Success($"Deleted {moviesToDelete.Count} movies. {moviesWithReviews.Count} movies were not deleted due to existing reviews."));
    }
}
