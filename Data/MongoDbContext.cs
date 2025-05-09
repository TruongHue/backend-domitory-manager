using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace API_dormitory.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        // ✅ Public IMongoClient để dùng transaction
        public IMongoClient Client { get; }

        public MongoDbContext(IConfiguration config)
        {
            string connectionString = config["MongoDB:ConnectionString"];
            string databaseName = config["MongoDB:DatabaseName"];

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "⚠ ConnectionString không được để trống!");

            if (string.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException(nameof(databaseName), "⚠ DatabaseName không được để trống!");

            Client = new MongoClient(connectionString); // gán cho property Client
            _database = Client.GetDatabase(databaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
}
