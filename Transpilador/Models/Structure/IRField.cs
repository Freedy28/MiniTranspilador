using Transpilador.Models.Base;

namespace Transpilador.Models.Structure
{
    public class IRField
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public IRAccessModifier AccessModifier { get; set; }
        public bool IsStatic { get; set; }
        public IRExpression InitialValue { get; set; }

        public IRField(string name, string type)
        {
            Name = name;
            Type = type;
            AccessModifier = IRAccessModifier.Private;
            IsStatic = false;
            InitialValue = null;
        }
    }
}
