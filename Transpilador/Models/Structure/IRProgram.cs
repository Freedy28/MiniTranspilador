namespace Transpilador.Models.Structure
{
    public class IRProgram
    {
        public List<IRClass> Classes { get; set; }
        public string Namespace { get; set; }

        public IRProgram()
        {
            Classes = [];
            Namespace = "";
        }
    }
}