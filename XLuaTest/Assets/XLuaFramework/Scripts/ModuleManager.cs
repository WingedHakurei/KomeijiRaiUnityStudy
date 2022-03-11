using System;
using UnityEngine;

/// <summary>
/// 模块管理器 工具类
/// </summary>
public class ModuleManager : Singleton<ModuleManager>
{
    /// <summary>
    /// 加载一个模块 唯一对外API函数
    /// </summary>
    /// <param name="moduleConfig"></param>
    /// <param name="moduleAction"></param>
    public void Load(ModuleConfig moduleConfig, Action<bool> moduleAction)
    {
        if (GlobalConfig.HotUpdate == false)
        {
            if (GlobalConfig.BundleMode == false)
            {
                moduleAction(true);
            }
            else
            {
                LoadAssetBundleConfig(moduleConfig, moduleAction);
            }
        }
        else
        {
            Downloader.Instance.Download(moduleConfig, (downloadResult) =>
            {
                if (downloadResult == true)
                {
                    if (GlobalConfig.BundleMode == true)
                    {
                        LoadAssetBundleConfig(moduleConfig, moduleAction);
                    }
                    else
                    {
                        Debug.LogError("配置错误！HotUpdate == true && BundleMode == false");
                    }
                }
                else
                {
                    Debug.LogError("下载失败！");
                }
            });
        }
    }
    private void LoadAssetBundleConfig(ModuleConfig moduleConfig, Action<bool> moduleAction)
    {
        AssetLoader.Instance.LoadAssetBundleConfig(moduleConfig.moduleName, (assetConfigResult) =>
        {
            if (assetConfigResult == true)
            {
                moduleAction(true);
            }
            else
            {
                Debug.LogError("LoadAssetBundleConfig 出错！");
            }
        });
    }
}
