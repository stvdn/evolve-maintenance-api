namespace Application.DTOs.Common;

public class ApiResponse<T>
{
    public string Status { get; set; } = "success";
    public T? Data { get; set; }
    public PaginationDto? Pagination { get; set; }

    public static ApiResponse<T> Success(T data) => new() { Data = data };

    public static ApiResponse<T> SuccessWithPagination(T data, PaginationDto pagination)
        => new() { Data = data, Pagination = pagination };
}

public class ErrorResponse
{
    public string Status { get; set; } = "error";
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public List<FieldError>? Errors { get; set; }

    public static ErrorResponse Create(string message, int statusCode)
        => new() { Message = message, StatusCode = statusCode };

    public static ErrorResponse Validation(List<FieldError> errors)
        => new() { Message = "Error de validación", StatusCode = 400, Errors = errors };
}

public class FieldError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class PaginationDto
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / Limit);
}
