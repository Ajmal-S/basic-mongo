using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IO;

namespace ConsoleApplication1
{
    class Program
    {
        protected const string FilePathIndexingCollectionName = "filePathIndexCollection";
        protected const string TestCaseNameIndexingCollectionName = "testNameIndexCollection";
        protected const string FileIdTestIdCollectionName = "fileIndexAndTestNameIndexCollection";
        private static string fileIndexId = "";
        private static List<string> TestIdList = new List<string>();
        private static List<string> FileNameList = new List<string>();
        private static List<string> TestNameList = new List<string>();
        static void Main(string[] args)
        {

            string mongoDbUrl = "mongodb://vbothra:varun@ds041593.mongolab.com:41593/rtsdb";
            var MongoDbClient = new MongoClient(mongoDbUrl);
            var Database = MongoDbClient.GetDatabase("rtsdb");
            var FileIndexCollection = Database.GetCollection<BsonDocument>(FilePathIndexingCollectionName);
            string k = @"C:\\a\\Source\\Core\\Core\\FileAppender.cs".GetHashCode().ToString();
            var TestNameIndexCollection = Database.GetCollection<BsonDocument>(TestCaseNameIndexingCollectionName);
            var FileIndexAndTestNameIndexPairCollection =
                Database.GetCollection<BsonDocument>(FileIdTestIdCollectionName);
            var TestNameIndexPairCollection =
                Database.GetCollection<BsonDocument>(TestCaseNameIndexingCollectionName);
            var findFilter = Builders<BsonDocument>.Filter.Eq("FilePath", "C:\\a\\Source\\Core\\Core\\PlatformDispatcherTimer.cs");
            GetData(FileIndexCollection, findFilter).Wait();
            Console.WriteLine(fileIndexId);
            var filter = Builders<BsonDocument>.Filter.Eq("FileId", fileIndexId);
            GetDataForFile(FileIndexAndTestNameIndexPairCollection, filter).Wait();
            foreach (var s in TestIdList)
            {
                var filter2 = Builders<BsonDocument>.Filter.Eq("_id", s);
                GetDataForTests(TestNameIndexCollection, filter2).Wait();
            }
            Console.WriteLine("operation done");
            File.AppendAllLines(@"e:\testnames.txt", TestNameList);


#if DEBUG
            Console.ReadLine();
#endif

        }

        private static async Task GetData(IMongoCollection<BsonDocument> FileIndexCollection,
            FilterDefinition<BsonDocument> findFilter)
        {
            await FileIndexCollection.Find(findFilter).ForEachAsync(data =>
                fileIndexId = data["_id"].ToString());
        }

        private static async Task GetDataForFile(IMongoCollection<BsonDocument> FileIndexAndTestNameIndexPairCollection,
            FilterDefinition<BsonDocument> findFilter)
        {
            var result = await FileIndexAndTestNameIndexPairCollection.Find(findFilter).ToListAsync();
            foreach (var res in result)
            {
                TestIdList.Add(res["TestId"].ToString());
            }
            TestIdList = TestIdList.Distinct().ToList();
        }

        private static async Task GetDataForTests(IMongoCollection<BsonDocument> FileIndexAndTestNameIndexPairCollection,
            FilterDefinition<BsonDocument> findFilter)
        {
            await FileIndexAndTestNameIndexPairCollection.Find(findFilter).ForEachAsync(data =>
                TestNameList.Add(data["TestName"].ToString()));

        }
    }
}
