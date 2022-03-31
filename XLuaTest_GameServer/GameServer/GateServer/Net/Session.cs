using System;
using System.Threading.Tasks;
using Common;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using IGrains;
using LaunchPB;
using Orleans;

namespace GateServer.Net
{
    /// <summary>
    /// 网关服务器跟游戏客户端的一个链路 Session 对象
    /// </summary>
    public class Session
    {
        /// <summary>
        /// 代表这个链路的 IChannelHandlerContext 对象
        /// </summary>
        private IChannelHandlerContext context;

        /// <summary>
        /// 这个 Session 的观察者对象，Silo 节点可以通过这个观察者对象向这个观察者对象发消息
        /// </summary>
        private PacketObserver packetObserver;

        /// <summary>
        /// 本进程（GateServer）向 Silo 节点发送数据包的目标 Actor
        /// </summary>
        private IPacketRouterGrain routerGrain;

        /// <summary>
        /// 标记这个 Session 对象是否经过鉴权
        /// </summary>
        private bool isLogin;

        /// <summary>
        /// 这个网关服务器和 Silo 主机节点之间的链接
        /// </summary>
        private IClusterClient client;

        public Session(IClusterClient client, IChannelHandlerContext context)
        {
            this.client = client;

            this.context = context;

            isLogin = false;
        }

        /// <summary>
        /// 处理收到的来自游戏客户端的数据包
        /// </summary>
        /// <param name="netPackage"></param>
        /// <returns></returns>
        public async Task DispatchReceivePacket(NetPackage netPackage)
        {
            try
            {
                // 未鉴权的 Session 对象 只能接收 Register 和 Login 这两类消息

                if (isLogin == false)
                {
                    ILoginGrain loginGrain = client.GetGrain<ILoginGrain>("SingleLoginGrain");

                    if (netPackage.protoID == (int)ProtoCode.ERegister)
                    {
                        NetPackage resultPackage = await loginGrain.OnRegister(netPackage);

                        await context.WriteAndFlushAsync(resultPackage);
                    }
                    else if (netPackage.protoID == (int)ProtoCode.ELogin)
                    {
                        NetPackage resultPackage = await loginGrain.OnLogin(netPackage);

                        await context.WriteAndFlushAsync(resultPackage);

                        await NotifyOnLine(resultPackage);
                    }
                    else
                    {
                        Logger.Instance.Information($"未被鉴权的 Session 发送协议：{netPackage.protoID}");
                    }
                }
                else
                {
                    await routerGrain.OnReceivePacket(netPackage);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e.ToString());
            }
        }

        /// <summary>
        /// 通知 Silo 主机节点 某个账号上线
        /// </summary>
        /// <param name="netPackage"></param>
        /// <returns></returns>
        private async Task NotifyOnLine(NetPackage resultPackage)
        {
            IMessage message = new LoginResp();

            LoginResp loginResp = message.Descriptor.Parser.ParseFrom(
                resultPackage.bodyData, 0, resultPackage.bodyData.Length) as LoginResp;

            if (loginResp.Result == LoginResult.ELoginSuccess)
            {
                isLogin = true;

                routerGrain = client.GetGrain<IPacketRouterGrain>(loginResp.Account);

                packetObserver = new PacketObserver(context);

                IPacketObserver observerRef = await client.CreateObjectReference<IPacketObserver>(packetObserver);

                await routerGrain.BindPacketObserver(observerRef);

                await routerGrain.OnLine();
            }
        }

        /// <summary>
        /// 通知 Silo 主机节点 某个账号下线
        /// </summary>
        /// <returns></returns>
        public void Disconnect()
        {
            routerGrain?.OffLine();
        }
    }
}