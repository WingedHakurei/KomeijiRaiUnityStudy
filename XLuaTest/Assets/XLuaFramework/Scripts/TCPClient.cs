using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
/// <summary>
/// 数据包类型
/// </summary>
public enum PacketType
{
    /// <summary>
    /// 包类型未被初始化
    /// </summary>
    None = 0,

    /// <summary>
    /// 连接服务器成功
    /// </summary>
    ConnectSuccess = 1,

    /// <summary>
    /// 连接服务器失败
    /// </summary>
    ConnectFailed = 2,

    /// <summary>
    /// 收到新的TCP数据包
    /// </summary>
    TcpPacket = 3,

    /// <summary>
    /// 服务器连接断开
    /// </summary>
    ConnectDisconnect = 4,
}

/// <summary>
/// 网络包定义
/// </summary>
public class NetPacket
{
    /// <summary>
    /// 网络包构造函数
    /// </summary>
    /// <param name="packetType"></param>
    public NetPacket(PacketType packetType)
    {
        this.packetType = packetType;
        protoCode = 0;
        currRecv = 0;
    }

    /// <summary>
    /// 包的类型
    /// </summary>
    public PacketType packetType = PacketType.None;

    /// <summary>
    /// 如果包类型是 TcpPacket，则表示这个包的协议号 否则无意义
    /// </summary>
    public int protoCode = 0;

    /// <summary>
    /// 如果是在接收包头时，指的是包头收到多少字节了，如果是在接收包体时，指的是包体收到多少字节了
    /// </summary>
    public int currRecv = 0;

    /// <summary>
    /// 包头数据 接收时调用
    /// </summary>
    public byte[] PacketHeaderBytes = null;

    /// <summary>
    /// 包体数据 接收时调用
    /// </summary>
    public byte[] PacketBodyBytes = null;

    /// <summary>
    /// 包完整数据 发送时调用
    /// </summary>
    public byte[] PacketBytes = null;

    /// <summary>
    /// 定义一个配置变量，包头占用8个字节
    /// 1. 前4个字节表示包体的长度（不包含包头部分）
    /// 2. 后4个字节表示这个包的协议号
    /// </summary>
    public static int HEADER_SIZE = 8;
}

/// <summary>
/// 网络包队列 线程安全
/// </summary>
public class PacketQueue
{
    private Queue<NetPacket> netPackets = new Queue<NetPacket>();

    /// <summary>
    /// 网络包入队列
    /// </summary>
    /// <param name="netPacket"></param>
    public void Enqueue(NetPacket netPacket)
    {
        lock (netPacket)
        {
            netPackets.Enqueue(netPacket);
        }
    }

    /// <summary>
    /// 网络包出队列
    /// </summary>
    /// <returns></returns>
    public NetPacket Dequeue()
    {
        lock (netPackets)
        {
            if (netPackets.Count > 0)
            {
                return netPackets.Dequeue();
            }

            return null;
        }
    }

    /// <summary>
    /// 清空网络包队列
    /// </summary>
    public void Clear()
    {
        lock (netPackets)
        {
            netPackets.Clear();
        }
    }
}

/// <summary>
/// TCP客户端类
/// </summary>
public class TCPClient
{
    /// <summary>
    /// 请求连接服务器，这个函数在主线程调用
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    public void Connect(string address, int port)
    {
        lock (this)
        {
            if (socketState == false)
            {
                try
                {
                    Socket skt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    skt.BeginConnect(address, port, ConnectCallBack, skt);
                }
                catch (Exception)
                {
                    packetQueue.Enqueue(new NetPacket(PacketType.ConnectFailed));
                }
            }
        }
    }

    /// <summary>
    /// 主线程主动取走队列中的所有网络包
    /// </summary>
    /// <returns></returns>
    public List<NetPacket> GetPackets()
    {
        List<NetPacket> packetList = new List<NetPacket>();

        NetPacket one = packetQueue.Dequeue();
        while (one != null)
        {
            packetList.Add(one);
            one = packetQueue.Dequeue();
        }

        return packetList;
    }

    /// <summary>
    /// 主线程调用，发送网络包
    /// </summary>
    /// <param name="pCode"></param>
    /// <param name="body"></param>
    public void SendAsync(int pCode, byte[] body)
    {
        byte[] protoCode = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(pCode));
        byte[] bodySize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(body.Length));
        byte[] package = new byte[bodySize.Length + protoCode.Length + body.Length];
        Array.Copy(bodySize, 0, package, 0, bodySize.Length);
        Array.Copy(protoCode, 0, package, bodySize.Length, protoCode.Length);
        Array.Copy(body, 0, package, bodySize.Length + protoCode.Length, body.Length);

        SendAsync(package);
    }

    /// <summary>
    /// 主线程调用，发送网络字节流
    /// </summary>
    /// <param name="bytes"></param>
    private void SendAsync(byte[] bytes)
    {
        lock (this)
        {
            try
            {
                if (socketState == true)
                {
                    socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, SendCallBack, socket);
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }
    }

    public void SendCallBack(IAsyncResult asyncResult)
    {
        lock (this)
        {
            try
            {
                Socket socket = (Socket)asyncResult.AsyncState;

                socket.EndSend(asyncResult);
            }
            catch (Exception)
            {
                Disconnect();
            }

        }
    }

    /// <summary>
    /// 请求连接服务器的回调函数
    /// </summary>
    /// <param name="asyncResult"></param>
    public void ConnectCallBack(IAsyncResult asyncResult)
    {
        lock (this)
        {
            if (socketState == true)
            {
                return;
            }

            try
            {
                // 连接成功
                socket = (Socket)asyncResult.AsyncState;

                socketState = true;

                socket.EndConnect(asyncResult);

                packetQueue.Enqueue(new NetPacket(PacketType.ConnectSuccess));

                // 开始接收数据包包头
                ReadPacket();
            }
            catch (Exception)
            {
                socket = null;

                socketState = false;

                packetQueue.Enqueue(new NetPacket(PacketType.ConnectFailed));
            }
        }
    }

    /// <summary>
    /// 接收到数据包包头的回调函数
    /// </summary>
    /// <param name="asyncResult"></param>
    public void ReceiveHeader(IAsyncResult asyncResult)
    {
        lock (this)
        {
            try
            {
                NetPacket netPacket = (NetPacket)asyncResult.AsyncState;

                // 实际读取到的字节数
                int readSize = socket.EndReceive(asyncResult);

                // 服务器主动断开网络
                if (readSize == 0)
                {
                    Disconnect();

                    return;
                }

                netPacket.currRecv += readSize;

                if (netPacket.currRecv == NetPacket.HEADER_SIZE)
                {
                    // 收到了约定的包头的长度，重置下标记，后面准备接收包体
                    netPacket.currRecv = 0;

                    // 此包的包体大小
                    int bodySize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(netPacket.PacketHeaderBytes, 0));

                    // 此包的协议号
                    int protoCode = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(netPacket.PacketHeaderBytes, 4));
                    netPacket.protoCode = protoCode;

                    // 注意：有些协议确实没有包体部分，比如心跳 此时 bodySize == 0
                    if (bodySize < 0)
                    {
                        Disconnect();
                        return;
                    }

                    // 开始接收包体
                    netPacket.PacketBodyBytes = new byte[bodySize];

                    if (bodySize == 0)
                    {
                        packetQueue.Enqueue(netPacket);
                        // 开始读取下一次包
                        ReadPacket();
                        return;
                    }

                    socket.BeginReceive(netPacket.PacketBodyBytes, 0, bodySize, SocketFlags.None, ReceiveBody, netPacket);
                }
                else
                {
                    // 包头数据还没有收完，继续接收包头
                    int remainSize = NetPacket.HEADER_SIZE - netPacket.currRecv;
                    socket.BeginReceive(netPacket.PacketBodyBytes, netPacket.currRecv, remainSize, SocketFlags.None, ReceiveBody, netPacket);
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }
    }

    /// <summary>
    /// 接收到的数据包包体的回调函数
    /// </summary>
    /// <param name="asyncResult"></param>
    public void ReceiveBody(IAsyncResult asyncResult)
    {
        lock (this)
        {
            try
            {
                NetPacket netPacket = (NetPacket)asyncResult.AsyncState;

                int readSize = socket.EndReceive(asyncResult);

                if (readSize == 0)
                {
                    // 服务器主动断开网络
                    Disconnect();
                    return;
                }

                netPacket.currRecv += readSize;

                if (netPacket.currRecv == netPacket.PacketBodyBytes.Length)
                {
                    // 收到了约定的包体长度 重置下标记
                    netPacket.currRecv = 0;

                    packetQueue.Enqueue(netPacket);

                    // 开始读取下一次包
                    ReadPacket();
                }
                else
                {
                    // 没有收到足够的包体长度，继续收包体
                    int remainSize = netPacket.PacketBodyBytes.Length - netPacket.currRecv;
                    socket.BeginReceive(netPacket.PacketBodyBytes, netPacket.currRecv, remainSize, SocketFlags.None, ReceiveBody, netPacket);
                }

            }
            catch (Exception)
            {
                Disconnect();
            }
        }
    }

    /// <summary>
    /// 断开网络连接，有可能是IO线程调用，也可能是主线程调用
    /// </summary>
    public void Disconnect()
    {
        lock (this)
        {
            if (socketState == true)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch (System.Exception)
                {
                    socket.Close();

                    socket = null;
                    socketState = false;
                    packetQueue.Clear();

                    packetQueue.Enqueue(new NetPacket(PacketType.ConnectDisconnect));
                }
            }
        }
    }

    private void ReadPacket()
    {
        // 创建一个TCP空包
        NetPacket netPacket = new NetPacket(PacketType.TcpPacket);

        // 约定的是包头8个字节
        netPacket.PacketHeaderBytes = new byte[NetPacket.HEADER_SIZE];

        // 开始接收远端发来的数据包头
        socket.BeginReceive(netPacket.PacketHeaderBytes, 0, NetPacket.HEADER_SIZE, SocketFlags.None, ReceiveHeader, netPacket);
    }

    /// <summary>
    /// 这个 TCPClient 对象管理的客户端 socket
    /// </summary>
    private Socket socket = null;

    /// <summary>
    /// 推送给主线程接收的网络包队列
    /// </summary>
    private PacketQueue packetQueue = new PacketQueue();


    /// <summary>
    /// 当前网络状态 true 是已连接 false 是未连接
    /// </summary>
    private bool socketState = false;
}