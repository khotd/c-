namespace project.Models.DTO;

public class ErrorResponse
{
    public string Error { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? TraceId { get; set; }
}
