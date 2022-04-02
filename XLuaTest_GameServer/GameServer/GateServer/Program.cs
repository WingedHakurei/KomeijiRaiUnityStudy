using System;
using System.Threading.Tasks;
using Common;
using GateServer.Net;
using Orleans;
using Orleans.Configuration;

namespace GateServer
{
    class Program
    {
        private static IClusterClient client;

        private static TcpServer tcpServer;

        static async Task Main(string[] args)
        {
            Logger.Create("GateServer");

            await ConnectClient();

            Logger.Instance.Information("网关服务器（GateServer）连接游戏服务器（CardServer）成功！");

            tcpServer = new TcpServer(client);

            await tcpServer.StartAsync();

            Console.ReadKey();
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
