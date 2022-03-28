using System;
using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Google.Protobuf;

namespace GateServer.Net
{
    /// <summary>
    /// 实现 TcpServer 的解码器
    /// </summary>
    public class TcpServerDecoder : ByteToMessageDecoder
    {
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                // 网络包的包头约定的是8个字节

                if (input.ReadableBytes < 8)
                {
                    return;
                }

                // 获取下包体的长度

                int bodyLength = input.GetInt(input.ReaderIndex);

                if (bodyLength < 0 || bodyLength > (1024 * 8))
                {
                    // 包体长度不合法

                    context.CloseAsync();

                    return;
                }

                // 检查现在长度是不是够一个完整的网络包

                if (input.ReadableBytes < (8 + bodyLength))
                {
                    // 还不够一个完整的网络包长度 等待下次重新接收

                    return;
                }

                // 读取包头中记录包体长度的部分

                input.ReadInt();

                // 读取包头中记录包体长度的部分

                int protoID = input.ReadInt();

                // 读取包体部分

                byte[] bodyData = new byte[bodyLength];

                input.ReadBytes(bodyData);

                if (protoID == (int)LaunchPB.ProtoCode.EHero)
                {
                    // 将包体字节流反序列化成PB对象

                    IMessage message = new LaunchPB.Hero();

                    LaunchPB.Hero hero = message.Descriptor.Parser.ParseFrom(bodyData, 0, bodyLength) as LaunchPB.Hero;

                    // 将PB对象包装成 TcpMessage 对象，然后放到 output 队列中

                    TcpMessage oneMessage = new TcpMessage()
                    {
                        protoID = protoID,
                        message = message,
                        type = typeof(LaunchPB.Hero)
                    };

                    output.Add(oneMessage);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("解析数据异常，" + e.Message + "\n" + e.StackTrace);
            }
        }
    }
}