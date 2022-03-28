using System;
using DotNetty.Transport.Channels;

namespace GateServer.Net
{
    /// <summary>
    /// 实现 TcpServer 的处理器
    /// </summary>
    public class TcpServerHandler : SimpleChannelInboundHandler<TcpMessage>
    {
        protected override void ChannelRead0(IChannelHandlerContext context, TcpMessage msg)
        {
            Console.WriteLine($"{context.Channel.RemoteAddress.ToString()} 收到协议 {msg.type} 数据！");

            if (msg.type == typeof(LaunchPB.Hero))
            {
                LaunchPB.Hero hero = msg.message as LaunchPB.Hero;

                hero.Name = "KomeijiRai";

                hero.Age = 22;

                // 返回给客户端

                TcpMessage respMessage = new TcpMessage()
                {
                    protoID = msg.protoID,
                    message = hero,
                    type = typeof(LaunchPB.Hero)
                };

                context.WriteAndFlushAsync(respMessage);
            }
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);

            Console.WriteLine($"{context.Channel.RemoteAddress.ToString()} 连接成功！");
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);

            Console.WriteLine($"{context.Channel.RemoteAddress.ToString()} 连接断开！");
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            base.ExceptionCaught(context, exception);

            Console.WriteLine($"{context.Channel.RemoteAddress.ToString()} 连接异常 {exception}！");
        }
    }
}