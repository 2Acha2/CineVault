namespace CineVault.API.DTOs;

public record ApiResponseDto<TResponseData>(
    int StatusCode,
    string Message,
    TResponseData Data,
    DateTime Timestamp,
    string CorrelationId,
    List<string> Errors
)
{
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

    public static ApiResponseDto<TResponseData> Success(TResponseData data, string message = "Success", int statusCode = 200, string correlationId = null)
    {
        return new(statusCode, message, data, DateTime.UtcNow, correlationId ?? Guid.NewGuid().ToString(), new());
    }

    public static ApiResponseDto<TResponseData> Failure(string message, int statusCode = 400, List<string> errors = null, string correlationId = null)
    {
        return new(statusCode, message, default, DateTime.UtcNow, correlationId ?? Guid.NewGuid().ToString(), errors ?? new());
    }
}
