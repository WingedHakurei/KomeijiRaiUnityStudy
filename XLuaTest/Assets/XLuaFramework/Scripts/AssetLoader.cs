using System;
using UnityEngine;

/// <summary>
/// 模块资源加载器
/// </summary>
public class AssetLoader : Singleton<AssetLoader>
{
    /// <summary>
    /// 加载模块对应的全局AssetBundle资源管理文件
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="action"></param>
    public void LoadAssetBundleConfig(string moduleName, Action<bool> action)
    {

    }
}
