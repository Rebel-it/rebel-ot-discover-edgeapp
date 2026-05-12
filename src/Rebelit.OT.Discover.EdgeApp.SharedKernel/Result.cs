namespace Rebelit.OT.Discover.EdgeApp.SharedKernel;

public class Result<T>
{
    public T? Data { get; set; }
    public bool Success => string.IsNullOrEmpty(ErrorMessage);
    public string? ErrorMessage { get; set; }

}