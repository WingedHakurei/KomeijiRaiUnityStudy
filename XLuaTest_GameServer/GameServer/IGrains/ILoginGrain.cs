using System.Threading.Tasks;
using Orleans;

namespace IGrains
{
    public interface ILoginGrain : IGrainWithStringKey
    {
        /// <summary>
        /// 请求登录
        /// </summary>
        /// <param name="netPackage"></param>
        /// <returns></returns>
        Task<NetPackage> OnLogin(NetPackage netPackage);

        /// <summary>
        /// 请求注册
        /// </summary>
        /// <param name="netPackage"></param>
        /// <returns></returns>
        Task<NetPackage> OnRegister(NetPackage netPackage);
    }
}