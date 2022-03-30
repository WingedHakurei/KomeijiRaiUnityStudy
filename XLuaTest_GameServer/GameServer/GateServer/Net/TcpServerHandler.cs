using System;
using Common;
using DotNetty.Transport.Channels;
using IGrains;
using Orleans;

namespace GateServer.Net
{
    /// <summary>
    /// 实现 TcpServer 的处理器
    /// </summary>
    public class TcpServerHandler : SimpleChannelInboundHandler<NetPackage>
    {
        private readonly IClusterClient client;

        private IPacketRouterGrain routerGrain;

        private PacketObserver packetObserver;

        public TcpServerHandler(IClusterClient client)
        {
            this.client = client;
        }

        protected override void ChannelRead0(IChannelHandlerContext context, NetPackage netPackage)
        {
            routerGrain.OnReceivePacket(netPackage);
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);

            routerGrain = client.GetGrain<IPacketRouterGrain>(123);

            packetObserver = new PacketObserver(context);

            IPacketObserver observerRef = client.CreateObjectReference<IPacketObserver>(packetObserver).Result;

            routerGrain.BindPacketObserver(observerRef).Wait();

            Logger.Instance.Information($"{context.Channel.RemoteAddress.ToString()} 连接成功！");
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);

            Logger.Instance.Information($"{context.Channel.RemoteAddress.ToString()} 连接断开！");
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            base.ExceptionCaught(context, exception);

            Logger.Instance.Information($"{context.Channel.RemoteAddress.ToString()} 连接异常 {exception}！");
        }
    }
}