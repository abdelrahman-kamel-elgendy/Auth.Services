namespace AuthService.API.Models;

public sealed class ApiResponse<T>(bool success, T? data, string? message)
{
    public bool Success { get; } = success;
    public T? Data { get; } = data;
    public string? Message { get; set; } = message;
    public Dictionary<string, List<string>>? ValidationErrors { get; set; }
    public List<string>? Errors { get; set; }
}
