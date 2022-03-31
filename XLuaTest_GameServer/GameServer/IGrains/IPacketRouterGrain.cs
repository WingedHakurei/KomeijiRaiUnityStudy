using System.Threading.Tasks;
using Orleans;

namespace IGrains
{
    public interface IPacketRouterGrain : IGrainWithStringKey
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

        /// <summary>
        /// 网关通知：当前 Grain 对应的玩家上线了
        /// </summary>
        /// <returns></returns>
        Task OnLine();

        /// <summary>
        /// 网关通知：当前 Grain 对应的玩家掉线了
        /// </summary>
        /// <returns></returns>
        Task OffLine();
    }
}