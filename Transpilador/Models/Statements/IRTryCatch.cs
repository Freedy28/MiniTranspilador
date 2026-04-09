
using Transpilador.Models.Base;

namespace Transpilador.Models.Statements;

public class IRTryCatch : IRStatement
{
    public List<IRStatement> TryBody { get; set; } = [];
    public List<IRCatchClause> CatchClauses { get; set; } = [];
    public List<IRStatement>? FinallyBody { get; set; }
}

public class IRCatchClause
{
    public string ExceptionType { get; set; } = "Exception";
    public string VariableName { get; set; } = "e";
    public List<IRStatement> Body { get; set; } = [];
}
