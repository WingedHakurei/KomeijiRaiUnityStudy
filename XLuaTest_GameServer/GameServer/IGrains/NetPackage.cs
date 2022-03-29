using System;

namespace IGrains
{
    /// <summary>
    /// 在 GateServer 和 CardServer 之间传递的网络包格式
    /// </summary>
    public class NetPackage
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public int protoID;

        /// <summary>
        /// 消息体
        /// </summary>
        public byte[] bodyData;
    }
}
