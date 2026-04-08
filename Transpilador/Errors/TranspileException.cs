namespace Transpilador.Errors;

public class TranspileException : Exception
{
    public string ErrorType { get; }
    public List<ErrorDetail> Details { get; }

    public TranspileException(string message, string errorType, List<ErrorDetail> details = null)
        : base(message)
    {
        ErrorType = errorType;
        Details = details ?? [];
    }
}
