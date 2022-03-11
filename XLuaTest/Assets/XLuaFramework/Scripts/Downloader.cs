using System;
using UnityEngine;

/// <summary>
/// 下载器 工具类
/// </summary>
public class Downloader : Singleton<Downloader>
{
    /// <summary>
    /// 根据模块的配置，下载对应的模块
    /// </summary>
    /// <param name="moduleConfig"></param>
    /// <param name="action"></param>
    public void Download(ModuleConfig moduleConfig, Action<bool> action)
    {

    }
}
