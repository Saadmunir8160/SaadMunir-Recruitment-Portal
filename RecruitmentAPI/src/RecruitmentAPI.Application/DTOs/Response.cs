namespace RecruitmentAPI.Application.DTOs;

public class Response<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static Response<T> SuccessResponse(T data, string message = "Operation completed successfully.")
        => new() { Success = true, Data = data, Message = message };

    public static Response<T> FailureResponse(string message)
        => new() { Success = false, Message = message };
}
