namespace CineVault.API.Models.Api;

public class ApiResponse<TResponseData>
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public TResponseData Data { get; set; }

    // Додаткові поля
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public List<string> Errors { get; set; } = new();

    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

    // Конструктори
    public ApiResponse() { }

    public ApiResponse(int statusCode, string message, TResponseData data, string correlationId = null)
    {
        StatusCode = statusCode;
        Message = message;
        Data = data;
        CorrelationId = correlationId ?? Guid.NewGuid().ToString();
        Timestamp = DateTime.UtcNow;
    }

    public static ApiResponse<TResponseData> Success(TResponseData data, string message = "Success", int statusCode = 200, string correlationId = null)
    {
        return new ApiResponse<TResponseData>(statusCode, message, data, correlationId);
    }

    public static ApiResponse<TResponseData> Failure(string message, int statusCode = 400, List<string> errors = null, string correlationId = null)
    {
        return new ApiResponse<TResponseData>(statusCode, message, default, correlationId)
        {
            Errors = errors ?? new List<string>()
        };
    }
}
