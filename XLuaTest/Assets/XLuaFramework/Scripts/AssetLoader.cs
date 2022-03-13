using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 模块资源加载器
/// </summary>
public class AssetLoader : Singleton<AssetLoader>
{
    /// <summary>
    /// 平台对应的只读路径下的资源
    /// Key 模块名字
    /// Value 模块所有的资源
    /// </summary>
    public Dictionary<string, Hashtable> base2Assets;

    /// <summary>
    /// 模块资源加载器的构造函数
    /// </summary>
    public AssetLoader()
    {
        base2Assets = new Dictionary<string, Hashtable>();
    }

    /// <summary>
    /// 根据模块的json配置文件 创建 内存中的资源容器
    /// </summary>
    /// <param name="moduleABConfig"></param>
    /// <returns></returns>
    public Hashtable ConfigAssembly(ModuleABConfig moduleABConfig)
    {
        Dictionary<string, BundleRef> name2BundleRef = new Dictionary<string, BundleRef>();

        foreach (KeyValuePair<string, BundleInfo> keyValue in moduleABConfig.BundleArray)
        {
            string bundleName = keyValue.Key;

            BundleInfo bundleInfo = keyValue.Value;

            name2BundleRef[bundleName] = new BundleRef(bundleInfo);
        }

        Hashtable path2AssetRef = new Hashtable();

        for (int i = 0; i < moduleABConfig.AssetArray.Length; i++)
        {
            AssetInfo assetInfo = moduleABConfig.AssetArray[i];

            // 装配一个 AssetRef 对象

            AssetRef assetRef = new AssetRef(assetInfo);

            assetRef.bundleRef = name2BundleRef[assetInfo.bundle_name];

            int count = assetInfo.dependencies.Count;

            assetRef.dependencies = new BundleRef[count];

            for (int index = 0; index < count; index++)
            {
                string bundleName = assetInfo.dependencies[index];

                assetRef.dependencies[index] = name2BundleRef[bundleName];
            }

            // 装配好了放到 path2AssetRef 容器中

            path2AssetRef.Add(assetInfo.asset_path, assetRef);
        }

        return path2AssetRef;
    }

    /// <summary>
    /// 加载模块对应的全局AssetBundle资源管理文件
    /// </summary>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    public async Task<ModuleABConfig> LoadAssetBundleConfig(string moduleName)
    {
#if UNITY_EDITOR
        if (GlobalConfig.BundleMode == false)
        {
            return null;
        }
        else
        {
            return await LoadAssetBundleConfig_Runtime(moduleName);
        }
#else
        return await LoadAssetBundleConfig_Runtime(moduleName);
#endif
    }

    public async Task<ModuleABConfig> LoadAssetBundleConfig_Runtime(string moduleName)
    {
        string url = Application.streamingAssetsPath + "/" + moduleName + "/" + moduleName.ToLower() + ".json";

        UnityWebRequest request = UnityWebRequest.Get(url);

        await request.SendWebRequest();

        if (string.IsNullOrEmpty(request.error) == true)
        {
            return JsonMapper.ToObject<ModuleABConfig>(request.downloadHandler.text);
        }

        return null;
    }
}
