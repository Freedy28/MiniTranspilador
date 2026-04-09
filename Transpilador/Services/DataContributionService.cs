using MongoDB.Driver;
using System.Threading.Tasks;
using Transpilador.Models.Dataset;

namespace Transpilador.Services
{
    public class DataContributionService
    {
        private readonly IMongoCollection<CodeContribution> _contributionsCollection;
        private readonly IMongoCollection<CodeHash> _hashesCollection;

        public DataContributionService(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            _contributionsCollection = database.GetCollection<CodeContribution>("contributions");
            _hashesCollection = database.GetCollection<CodeHash>("contribution_hashes");

            // Crear un índice único en el campo "hash" para evitar duplicados.
            // Esto solo se ejecuta una vez si el índice no existe.
            var indexKeysDefinition = Builders<CodeHash>.IndexKeys.Ascending(h => h.Hash);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<CodeHash>(indexKeysDefinition, indexOptions);
            _hashesCollection.Indexes.CreateOne(indexModel);
        }
        public async Task TryAddContributionAsync(string csharpCode, string javaCode)
        {
            
        }
    }
}