namespace Rebelit.OT.Discover.EdgeApp.Dto;

public class ResponseDto<T>
{
    public T? Data { get; set; }
    public bool Success => string.IsNullOrEmpty(ErrorMessage);
    public string? ErrorMessage { get; set; }
}