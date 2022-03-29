using DotNetty.Transport.Channels;
using IGrains;

namespace GateServer.Net
{
    /// <summary>
    /// GateServer 进程上的观察者对象，用来监听 CardServer 发来的消息
    /// </summary>
    public class PacketObserver : IPacketObserver
    {
        private readonly IChannelHandlerContext context;

        public PacketObserver(IChannelHandlerContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// 当 GateServer 收到来自 CardServer 的消息
        /// </summary>
        /// <param name="netPackage"></param>
        public void OnReceivePacket(NetPackage netPackage)
        {
            // 返回给客户端

            context.WriteAndFlushAsync(netPackage);
        }
    }
}