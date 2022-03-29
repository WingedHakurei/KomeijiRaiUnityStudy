using System.Threading.Tasks;
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
            await ConnectClient();

            tcpServer = new TcpServer(client);

            await tcpServer.StartAsync();
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
