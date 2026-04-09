using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Transpilador.Models.Dataset
{
    // Documento simple para la colección "contribution_hashes"
    public class CodeHash
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // El hash SHA-256 del código fuente C#.
        // Lo marcaremos como un índice único para que la base de datos
        // impida duplicados a nivel de hardware.
        [BsonElement("hash")]
        public string Hash { get; set; }
    }
}