namespace UserManagement.Application.Common;

public class ServiceResult
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public string? ErrorCode { get; }

    private ServiceResult(bool isSuccess, string message, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        ErrorCode = errorCode;
    }

    public static ServiceResult Success() =>
        new ServiceResult(true, string.Empty);

    public static ServiceResult Success(string message) =>
        new ServiceResult(true, message);

    public static ServiceResult Failure(string message, string? errorCode = null) =>
        new ServiceResult(false, message, errorCode);
}
public class ServiceResult<T>
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public T? Data { get; }
    public string? ErrorCode { get; }

    private ServiceResult(bool isSuccess, string message, T? data = default, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Data = data;
        ErrorCode = errorCode;
    }

    public static ServiceResult<T> Success(T data) =>
        new ServiceResult<T>(true, string.Empty, data);

    public static ServiceResult<T> Success(T data, string message) =>
        new ServiceResult<T>(true, message, data);

    public static ServiceResult<T> Failure(string message, string? errorCode = null) =>
        new ServiceResult<T>(false, message, default, errorCode);
}