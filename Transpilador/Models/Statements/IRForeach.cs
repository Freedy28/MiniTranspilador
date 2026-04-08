using Transpilador.Models.Base;

namespace Transpilador.Models.Statements
{
    public class IRForeach : IRStatement
    {
        public string ItemType { get; set; }
        public string ItemName { get; set; }
        public IRExpression Collection { get; set; }
        public List<IRStatement> Body { get; set; }

        public IRForeach(string itemType, string itemName, IRExpression collection)
        {
            ItemType = itemType;
            ItemName = itemName;
            Collection = collection;
            Body = [];
        }
    }
}