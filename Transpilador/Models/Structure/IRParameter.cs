namespace Transpilador.Models.Structure
{
    public class IRParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public IRParameter(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
