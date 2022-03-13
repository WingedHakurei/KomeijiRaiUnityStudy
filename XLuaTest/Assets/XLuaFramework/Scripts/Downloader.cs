using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 下载器 工具类
/// </summary>
public class Downloader : Singleton<Downloader>
{
    /// <summary>
    /// 根据模块的配置，下载对应的模块
    /// </summary>
    /// <param name="moduleConfig"></param>
    /// <returns></returns>
    public async Task<bool> Download(ModuleConfig moduleConfig)
    {
        UnityWebRequest request = UnityWebRequest.Get("test_url");

        await request.SendWebRequest();

        if (string.IsNullOrEmpty(request.error) == true)
        {
            return true;
        }

        return false;
    }
}
