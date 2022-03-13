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
                ModuleABConfig moduleABConfig = await AssetLoader.Instance.LoadAssetBundleConfig(moduleConfig.moduleName);

                if (moduleABConfig == null)
                {
                    return false;
                }

                Debug.Log("模块包含的AB包总数量：" + moduleABConfig.BundleArray.Count);

                Hashtable path2AssetRef = AssetLoader.Instance.ConfigAssembly(moduleABConfig);

                AssetLoader.Instance.base2Assets.Add(moduleConfig.moduleName, path2AssetRef);

                return true;
            }
        }
        else
        {
            return await Downloader.Instance.Download(moduleConfig);
        }
    }

}
