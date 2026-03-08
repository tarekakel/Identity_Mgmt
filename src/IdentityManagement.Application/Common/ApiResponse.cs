namespace IdentityManagement.Application.Common;

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

    public static ApiResponse Ok(string? message = null) => new()
    {
        Success = true,
        Message = message
    };

    public static ApiResponse Fail(string? message = null, IReadOnlyList<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors ?? Array.Empty<string>()
    };
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static new ApiResponse<T> Fail(string? message = null, IReadOnlyList<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors ?? Array.Empty<string>()
    };
}
