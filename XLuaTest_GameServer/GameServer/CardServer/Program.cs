using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace CardServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await StartSilo();

            Console.WriteLine("开启 CardServer！");

            Console.ReadLine();
        }

        private static async Task<ISiloHost> StartSilo()
        {
            var host = new SiloHostBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "ClusterId";
                    options.ServiceId = "ServiceId";
                })
                .ConfigureApplicationParts(parts =>
                {
                    parts.AddFromApplicationBaseDirectory().WithReferences();
                })
                .Build();

            // 启动当前 Silo

            await host.StartAsync();

            return host;
        }
    }
}
