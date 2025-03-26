using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace API_dormitory.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration config)
        {
            string connectionString = config["MongoDB:ConnectionString"];
            string databaseName = config["MongoDB:DatabaseName"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "⚠ ConnectionString không được để trống!");
            }

            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentNullException(nameof(databaseName), "⚠ DatabaseName không được để trống!");
            }

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
}
