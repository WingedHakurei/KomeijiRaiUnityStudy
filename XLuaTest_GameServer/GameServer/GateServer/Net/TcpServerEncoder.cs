using System;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Google.Protobuf;

namespace GateServer.Net
{
    public class TcpServerEncoder : MessageToByteEncoder<TcpMessage>
    {
        protected override void Encode(IChannelHandlerContext context, TcpMessage oneMessage, IByteBuffer output)
        {
            byte[] body = oneMessage.message.ToByteArray();

            output.WriteInt(body.Length);

            output.WriteInt(oneMessage.protoID);

            output.WriteBytes(body);

            Console.WriteLine($"{context.Channel.RemoteAddress.ToString()} 发送协议 {oneMessage.type} 数据！");
        }
    }
}