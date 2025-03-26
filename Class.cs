using MongoDB.Driver;
using System;
class Class
{
    static void Main()
    {
        string connectionString = "mongodb+srv://thuhue20031022:Thuhue22102003@cluster0.asr0t.mongodb.net/?retryWrites=true&w=majority";

        try
        {
            var client = new MongoClient(connectionString);
            var databases = client.ListDatabaseNames().ToList();
            Console.WriteLine("✅ Kết nối thành công! Danh sách database:");
            foreach (var db in databases)
            {
                Console.WriteLine($"- {db}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Lỗi kết nối: {ex.Message}");
        }
    }
}
