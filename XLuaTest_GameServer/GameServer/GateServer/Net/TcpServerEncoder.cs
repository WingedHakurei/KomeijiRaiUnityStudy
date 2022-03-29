using System;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using IGrains;

namespace GateServer.Net
{
    public class TcpServerEncoder : MessageToByteEncoder<NetPackage>
    {
        protected override void Encode(IChannelHandlerContext context, NetPackage netPackage, IByteBuffer output)
        {
            byte[] body = netPackage.bodyData;

            output.WriteInt(body.Length);

            output.WriteInt(netPackage.protoID);

            output.WriteBytes(body);

            Console.WriteLine($"{context.Channel.RemoteAddress.ToString()} 发送数据！");
        }
    }
}