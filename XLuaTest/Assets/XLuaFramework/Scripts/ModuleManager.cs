using System.Collections;
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
                return await LoadBase(moduleConfig.moduleName);
            }
        }
        else
        {
            await Downloader.Instance.Download(moduleConfig);

            bool baseOk = await LoadBase(moduleConfig.moduleName);

            bool updateOk = await LoadUpdate(moduleConfig.moduleName);

            if (baseOk == false && updateOk == false)
            {
                return false;
            }

            return true;
        }
    }

    private async Task<bool> LoadBase(string moduleName)
    {
        ModuleABConfig moduleABConfig = await AssetLoader.Instance.LoadAssetBundleConfig(
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
        ModuleABConfig moduleABConfig = await AssetLoader.Instance.LoadAssetBundleConfig(
            BaseOrUpdate.Update, moduleName, moduleName.ToLower() + ".json");

        if (moduleABConfig == null)
        {
            return false;
        }

        Debug.Log($"模块{moduleName}的可读可写路径 包含的AB包总数量：{moduleABConfig.BundleArray.Count}");

        Hashtable path2AssetRef = AssetLoader.Instance.ConfigAssembly(moduleABConfig);

        AssetLoader.Instance.base2Assets.Add(moduleName, path2AssetRef);

        return true;
    }

}
