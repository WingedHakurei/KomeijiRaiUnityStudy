using System.Threading.Tasks;
using Orleans;

namespace IGrains
{
    public interface IPacketRouterGrain : IGrainWithIntegerKey
    {
        /// <summary>
        /// 当 CardServer 收到来自 GateServer 的消息
        /// </summary>
        /// <param name="netPackage"></param>
        /// <returns></returns>
        Task OnReceivePacket(NetPackage netPackage);

        /// <summary>
        /// 给 CardServer 绑定一个观察者 observer，方便 CardServer 给 GateServer 推送消息
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        Task BindPacketObserver(IPacketObserver observer);
    }
}