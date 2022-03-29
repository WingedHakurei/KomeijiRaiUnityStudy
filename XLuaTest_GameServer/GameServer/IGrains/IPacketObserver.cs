using Orleans;

namespace IGrains
{
    public interface IPacketObserver : IGrainObserver
    {
        /// <summary>
        /// 当 GateServer 收到来自 CardServer 的消息
        /// </summary>
        /// <param name="netPackage"></param>
        void OnReceivePacket(NetPackage netPackage);
    }
}