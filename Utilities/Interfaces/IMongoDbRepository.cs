using FluentValidation;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Durable.Utilities
{
    public interface IMongoDbRepository
    { 
        IMongoCollection<BsonDocument> GetCollection();

        Task<List<BsonDocument>> GetQueryByCollection(
            IMongoCollection<BsonDocument> collection,
            string field,
            string value);

        Task<List<BsonDocument>> GetQueryByFilter(
            string field,
            string value);
    }
}
