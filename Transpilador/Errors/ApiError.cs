namespace Transpilador.Errors;

public class ApiError
{
    public string Type { get; init; }
    public string Message { get; init; }
    public List<ErrorDetail> Details { get; init; }
    public string TraceId { get; init; }

    public ApiError(string type, string message, List<ErrorDetail> details = null, string traceId = null)
    {
        Type = type;
        Message = message;
        Details = details;
        TraceId = traceId;
    }
}

public class ErrorDetail
{
    public string Code { get; init; }
    public string Description { get; init; }
    public int? Line { get; init; }
    public int? Column { get; init; }

    public ErrorDetail(string code, string description, int? line = null, int? column = null)
    {
        Code = code;
        Description = description;
        Line = line;
        Column = column;
    }
}
