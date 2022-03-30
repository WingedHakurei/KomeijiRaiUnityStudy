using System;
using System.Threading.Tasks;
using Common;
using Google.Protobuf;
using IGrains;

namespace Grains
{
    public class PacketRouterGrain : Orleans.Grain, IPacketRouterGrain
    {
        private IPacketObserver observer;

        /// <summary>
        /// 当 CardServer 收到来自 GateServer 的消息
        /// </summary>
        /// <param name="netPackage"></param>
        /// <returns></returns>
        public Task OnReceivePacket(NetPackage netPackage)
        {
            // 当前 Grain 的 key

            long id = GrainReference.GrainIdentity.PrimaryKeyLong;

            Logger.Instance.Information($"CardServer {id} 收到 NetPackage");

            // 将消息再发回客户端

            if (netPackage.protoID == (int)LaunchPB.ProtoCode.EHero)
            {
                IMessage message = new LaunchPB.Hero();

                LaunchPB.Hero hero = message.Descriptor.Parser
                    .ParseFrom(netPackage.bodyData, 0, netPackage.bodyData.Length) as LaunchPB.Hero;

                hero.Name = "KomeijiRai";

                hero.Age = 22;

                // 将数据包再发回到网关服务器

                observer.OnReceivePacket(netPackage);

                Logger.Instance.Information($"CardServer {id} 发送 NetPackage");
            }

            return Task.CompletedTask;
        }

        public Task BindPacketObserver(IPacketObserver observer)
        {
            this.observer = observer;

            return Task.CompletedTask;
        }
    }
}
