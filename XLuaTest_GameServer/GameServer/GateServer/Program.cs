using System.Threading.Tasks;
using GateServer.Net;

namespace GateServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await TcpServer.Start(8899);
        }
    }
}
