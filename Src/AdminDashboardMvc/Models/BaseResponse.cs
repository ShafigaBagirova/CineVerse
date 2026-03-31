namespace AdminDashboardMvc.Models;

public class BaseResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class BaseResponse<T> : BaseResponse
{
    public T? Data { get; set; }
}