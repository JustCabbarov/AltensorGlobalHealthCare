using System.Diagnostics;

namespace AltensorGlobalHealthCare.Application.Common;

public record ErrorDetail(string? Field, string Message, string? Code = null);

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public IReadOnlyList<ErrorDetail>? Errors { get; set; }
    public string? TraceId { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null, string? traceId = null) =>
        new()
        {
            Success = true,
            Data = data,
            Message = message,
            TraceId = traceId ?? Activity.Current?.Id
        };

    public static ApiResponse<T> Fail(
        string message,
        string? errorCode = null,
        IReadOnlyList<ErrorDetail>? errors = null,
        string? traceId = null) =>
        new()
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode,
            Errors = errors,
            TraceId = traceId ?? Activity.Current?.Id
        };
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}

