using FluentValidation;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace Durable.Utilities
{
    public class MongoDbRepository : IMongoDbRepository
    {
        public async Task<List<BsonDocument>> GetQueryByCollection(
            IMongoCollection<BsonDocument> collection,
            string field,
            string value)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(field, value);
           return await (await collection.FindAsync(filter)).ToListAsync();
        }

        public async Task<List<BsonDocument>> GetQueryByFilter(
            string field,
            string value)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(field, value);
            return await (await GetCollection().FindAsync(filter)).ToListAsync();
        }

        public IMongoCollection<BsonDocument> GetCollection()
        {
            var dbClient = new MongoClient(Environment.GetEnvironmentVariable("MongoDBConnect"));
            var database = dbClient.GetDatabase(Environment.GetEnvironmentVariable("MongoDB"));
            return database.GetCollection<BsonDocument>(Environment.GetEnvironmentVariable("MongoDBCollection"));
        }
    }
}
