using System;
using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using IGrains;

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

                // 包装成一个 NetPackage 对象

                NetPackage netPackage = new NetPackage()
                {
                    protoID = protoID,
                    bodyData = bodyData
                };

                output.Add(netPackage);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("解析数据异常，" + e.Message + "\n" + e.StackTrace);
            }
        }
    }
}