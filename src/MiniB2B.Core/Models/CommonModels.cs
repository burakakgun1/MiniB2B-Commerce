namespace MiniB2B.Core.Models;

public class ServiceResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ServiceResult Success(string? message = null) => new() { IsSuccess = true, Message = message };
    public static ServiceResult Failure(string error) => new() { IsSuccess = false, Message = error, Errors = new() { error } };
    public static ServiceResult Failure(List<string> errors) => new() { IsSuccess = false, Errors = errors, Message = errors.FirstOrDefault() };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static ServiceResult<T> Success(T data, string? message = null) => new() { IsSuccess = true, Data = data, Message = message };
    public new static ServiceResult<T> Failure(string error) => new() { IsSuccess = false, Message = error, Errors = new() { error } };
    public new static ServiceResult<T> Failure(List<string> errors) => new() { IsSuccess = false, Errors = errors, Message = errors.FirstOrDefault() };
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
