using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.DB;
using Common.ORM;
using GateServer.Net;
using Orleans;
using Orleans.Configuration;

namespace GateServer
{
    class Program
    {
        private static IClusterClient client;

        private static TcpServer tcpServer;

        private static Table<Person> personDB = new Table<Person>();

        static async Task Main(string[] args)
        {
            Logger.Create("GateServer");

            TestDB();

            await ConnectClient();

            tcpServer = new TcpServer(client);

            await tcpServer.StartAsync();

            Console.ReadKey();
        }

        private static void TestDB()
        {
            Person person = new Person("张三", 8, Guid.NewGuid().ToString(), GenderEnum.男);

            person.Students = new List<Person>()
            {
                new Person("张小三1", 8, Guid.NewGuid().ToString(), GenderEnum.男),
                new Person("张小三2", 8, Guid.NewGuid().ToString(), GenderEnum.男),
                new Person("张小三3", 8, Guid.NewGuid().ToString(), GenderEnum.男),
                new Person("张小三4", 8, Guid.NewGuid().ToString(), GenderEnum.男)
            };

            person.Pet = new Pet() { Name = "旺财", Age = 3 };

            personDB.Add(person);
        }

        /// <summary>
        /// 连接 CardServer 服务器
        /// </summary>
        /// <returns></returns>
        private static async Task<IClusterClient> ConnectClient()
        {
            client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "ClusterId";
                    options.ServiceId = "ServiceId";
                })
                .Build();

            await client.Connect();

            return client;
        }
    }
}
