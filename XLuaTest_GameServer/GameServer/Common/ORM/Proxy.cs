using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Common.ORM
{
    public static class Proxy<T>
    {
        /// <summary>
        /// 数据库表的名字
        /// </summary>
        private static string tableName;

        /// <summary>
        /// 数据库名字
        /// </summary>
        private static string databaseName;

        /// <summary>
        /// 数据库连接配置Url
        /// </summary>
        private static string mongoUrl;

        /// <summary>
        /// 数据库单个表的引用（即 mongo 的单个集合）
        /// </summary>
        private static IMongoCollection<T> collection;

        public static IMongoCollection<T> Collection => collection;

        /// <summary>
        /// 静态构造函数（注意：不允许出现访问控制符）
        /// </summary>
        static Proxy()
        {
            Init();
        }

        /// <summary>
        /// 单个表的初始化函数
        /// </summary>
        private static void Init()
        {
            databaseName = "TestMongoDB";

            mongoUrl = "mongodb://localhost:27017";

            tableName = typeof(T).Name;

            BsonClassMap.RegisterClassMap<T>(cm => cm.AutoMap());

            collection = new MongoClient(mongoUrl).GetDatabase(databaseName).GetCollection<T>(tableName);
        }
    }
}