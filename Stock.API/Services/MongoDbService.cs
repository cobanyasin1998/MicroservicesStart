using MongoDB.Driver;

namespace Stock.API.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration configuration)
        {
            MongoClient client = new MongoClient(configuration.GetConnectionString("MongoDB"));
            _database = client.GetDatabase("StockAPIDB");
        }

        public IMongoCollection<T> GetCollection<T>() => _database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());



    }
}
