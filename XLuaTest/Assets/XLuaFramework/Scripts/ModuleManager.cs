using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    /// <returns></returns>
    public async Task<bool> Load(ModuleConfig moduleConfig)
    {
        if (GlobalConfig.HotUpdate == false)
        {
            if (GlobalConfig.BundleMode == false)
            {
                return true;
            }
            else
            {
                bool baseBundleOK = await LoadBase_Bundle(moduleConfig.moduleName);

                if (baseBundleOK == false)
                {
                    return false;
                }

                return await LoadBase(moduleConfig.moduleName);
            }
        }
        else
        {
            await Downloader.Instance.Download(moduleConfig);

            bool updateBundleOK = await LoadUpdate_Bundle(moduleConfig.moduleName);

            if (updateBundleOK == false)
            {
                return false;
            }

            bool baseBundleOK = await LoadBase_Bundle(moduleConfig.moduleName);

            if (baseBundleOK == false)
            {
                return false;
            }

            bool updateOK = await LoadUpdate(moduleConfig.moduleName);

            return updateOK;
        }
    }

    private async Task<bool> LoadBase(string moduleName)
    {
        ModuleABConfig moduleABConfig = await AssetLoader.Instance.LoadModuleABConfig(
            BaseOrUpdate.Base, moduleName, moduleName.ToLower() + ".json");

        if (moduleABConfig == null)
        {
            return false;
        }

        Debug.Log($"模块{moduleName}的只读路径 包含的AB包总数量：{moduleABConfig.BundleArray.Count}");

        Hashtable path2AssetRef = AssetLoader.Instance.ConfigAssembly(moduleABConfig);

        AssetLoader.Instance.base2Assets.Add(moduleName, path2AssetRef);

        return true;
    }

    private async Task<bool> LoadUpdate(string moduleName)
    {
        ModuleABConfig moduleABConfig = await AssetLoader.Instance.LoadModuleABConfig(
            BaseOrUpdate.Update, moduleName, moduleName.ToLower() + ".json");

        if (moduleABConfig == null)
        {
            return false;
        }

        Debug.Log($"模块{moduleName}的可读可写路径 包含的AB包总数量：{moduleABConfig.BundleArray.Count}");

        Hashtable path2AssetRef = AssetLoader.Instance.ConfigAssembly(moduleABConfig);

        AssetLoader.Instance.update2Assets.Add(moduleName, path2AssetRef);

        return true;
    }

    private async Task<bool> LoadUpdate_Bundle(string moduleName)
    {
        ModuleABConfig moduleABConfig = await AssetLoader.Instance.LoadModuleABConfig(
            BaseOrUpdate.Update, moduleName, moduleName.ToLower() + ".json");

        if (moduleABConfig == null)
        {
            Debug.LogError("LoadUpdate_Bundle...");

            return false;
        }

        foreach (KeyValuePair<string, BundleInfo> keyValue in moduleABConfig.BundleArray)
        {
            string bundleName = keyValue.Key;

            BundleInfo bundleInfo = keyValue.Value;

            AssetLoader.Instance.name2BundleRef[bundleName] = new BundleRef(bundleInfo, BaseOrUpdate.Update);
        }

        return true;
    }

    private async Task<bool> LoadBase_Bundle(string moduleName)
    {
        ModuleABConfig moduleABConfig = await AssetLoader.Instance.LoadModuleABConfig(
            BaseOrUpdate.Base, moduleName, moduleName.ToLower() + ".json");

        if (moduleABConfig == null)
        {
            Debug.LogError("LoadBase_Bundle...");

            return false;
        }

        foreach (KeyValuePair<string, BundleInfo> keyValue in moduleABConfig.BundleArray)
        {
            string bundleName = keyValue.Key;

            if (AssetLoader.Instance.name2BundleRef.ContainsKey(bundleName) == false)
            {
                BundleInfo bundleInfo = keyValue.Value;

                AssetLoader.Instance.name2BundleRef[bundleName] = new BundleRef(bundleInfo, BaseOrUpdate.Base);
            }
        }

        return true;
    }

}
