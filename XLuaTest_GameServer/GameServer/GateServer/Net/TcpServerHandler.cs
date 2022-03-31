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

        private Session session;

        public TcpServerHandler(IClusterClient client)
        {
            this.client = client;
        }

        protected override async void ChannelRead0(IChannelHandlerContext context, NetPackage netPackage)
        {
            await session.DispatchReceivePacket(netPackage);
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);

            session = new Session(client, context);

            Logger.Instance.Information($"{context.Channel.RemoteAddress.ToString()} 连接成功！");
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);

            session.Disconnect();

            Logger.Instance.Information($"{context.Channel.RemoteAddress.ToString()} 连接断开！");
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            base.ExceptionCaught(context, exception);

            Logger.Instance.Information($"{context.Channel.RemoteAddress.ToString()} 连接异常 {exception}！");
        }
    }
}