using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Transpilador.Models.Dataset
{
    // Este es el documento principal que se guardará en la colección "contributions"
    public class CodeContribution
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("source_language")]
        public string SourceLanguage { get; set; } = "csharp";

        [BsonElement("target_language")]
        public string TargetLanguage { get; set; } = "java";

        [BsonElement("source_code")]
        public string SourceCode { get; set; }

        [BsonElement("target_code")]
        public string TargetCode { get; set; }

        [BsonElement("metadata")]
        public ContributionMetadata Metadata { get; set; }
    }

    public class ContributionMetadata
    {
        [BsonElement("description")]
        public string Description { get; set; } = "Generado automáticamente por el usuario.";

        [BsonElement("features")]
        public List<string> Features { get; set; } = new List<string>();

        [BsonElement("source")]
        public string Source { get; set; } = "user_submission";

        [BsonElement("timestamp_utc")]
        public DateTime TimestampUtc { get; set; }
    }
}