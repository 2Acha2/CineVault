using Mapster;
using CineVault.API.Entities;
using CineVault.API.DTOs;
using CineVault.API.Models.Api;

namespace CineVault.API.Mappings;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<Movie, MovieDto>.NewConfig();
        TypeAdapterConfig<Review, ReviewDto>.NewConfig();
        TypeAdapterConfig<User, UserDto>.NewConfig();

        TypeAdapterConfig<ApiRequest<MovieDto>, ApiRequestDto<MovieDto>>.NewConfig();
        TypeAdapterConfig<ApiRequest<UserDto>, ApiRequestDto<UserDto>>.NewConfig();
        TypeAdapterConfig<ApiRequest<ReviewDto>, ApiRequestDto<ReviewDto>>.NewConfig();

        TypeAdapterConfig<ApiResponse<MovieDto>, ApiResponseDto<MovieDto>>.NewConfig();
        TypeAdapterConfig<ApiResponse<UserDto>, ApiResponseDto<UserDto>>.NewConfig();
        TypeAdapterConfig<ApiResponse<ReviewDto>, ApiResponseDto<ReviewDto>>.NewConfig();
    }
}
