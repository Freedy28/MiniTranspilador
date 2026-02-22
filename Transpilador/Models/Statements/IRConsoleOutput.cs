using Transpilador.Models.Base;

namespace Transpilador.Models.Statements
{
    public class IRConsoleOutput : IRStatement
    {
        public IRExpression Argument { get; set; }  // null = Console.WriteLine() sin args
        public bool NewLine { get; set; }            // true = WriteLine, false = Write

        public IRConsoleOutput(IRExpression argument, bool newLine)
        {
            Argument = argument;
            NewLine = newLine;
        }
    }
}
