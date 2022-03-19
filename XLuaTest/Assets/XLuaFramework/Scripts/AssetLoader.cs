using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using LitJson;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 模块资源加载器
/// </summary>
public class AssetLoader : Singleton<AssetLoader>
{
    /// <summary>
    /// 克隆一个 GameObject 对象
    /// </summary>
    /// <param name="moduleName">模块名字</param>
    /// <param name="path">模块相对路径</param>
    /// <returns></returns>
    public GameObject Clone(string moduleName, string path)
    {
        AssetRef assetRef = LoadAssetRef<GameObject>(moduleName, path);

        if (assetRef == null || assetRef.asset == null)
        {
            return null;
        }

        GameObject gameObject = UnityEngine.Object.Instantiate(assetRef.asset) as GameObject;

        if (assetRef.children == null)
        {
            assetRef.children = new List<GameObject>();
        }

        assetRef.children.Add(gameObject);

        return gameObject;
    }

    /// <summary>
    /// 加载非 GameObject 对象并指定将要挂载的 GameObject 对象
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="assetPath"></param>
    /// <param name="gameObject"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T CreateAsset<T>(string moduleName, string assetPath, GameObject gameObject) where T : UnityEngine.Object
    {
        if (typeof(T) == typeof(GameObject) || (!string.IsNullOrEmpty(assetPath) && assetPath.EndsWith(".prefab")))
        {
            Debug.LogError("不可以加载 GameObject 类型，请直接使用 AssetLoader.Instance.Clone 方法，path：" + assetPath);

            return null;
        }

        if (gameObject == null)
        {
            Debug.LogError("CreateAsset 必须传递一个 gameObject 作为将要挂载的 GameObject 对象！");

            return null;
        }

        AssetRef assetRef = LoadAssetRef<T>(moduleName, assetPath);

        if (assetRef == null || assetRef.asset == null)
        {
            return null;
        }

        if (assetRef.children == null)
        {
            assetRef.children = new List<GameObject>();
        }

        assetRef.children.Add(gameObject);

        return assetRef.asset as T;
    }

    /// <summary>
    /// 加载 AssetRef 对象
    /// </summary>
    /// <param name="moduleName">模块名字</param>
    /// <param name="assetPath">模块相对路径</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private AssetRef LoadAssetRef<T>(string moduleName, string assetPath) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (GlobalConfig.BundleMode == false)
        {
            return LoadAssetRef_Editor<T>(moduleName, assetPath);
        }
        else
        {
            return LoadAssetRef_Runtime<T>(moduleName, assetPath);
        }
#else
        return LoadAssetRef_Runtime<T>(moduleName, assetPath);
#endif
    }

    /// <summary>
    /// 全局卸载函数
    /// </summary>
    /// <param name="module2Assets"></param>
    public void Unload(Dictionary<string, Hashtable> module2Assets)
    {
        foreach (string moduleName in module2Assets.Keys)
        {
            Hashtable path2AssetRef = module2Assets[moduleName];

            if (path2AssetRef == null)
            {
                continue;
            }

            foreach (AssetRef assetRef in path2AssetRef.Values)
            {
                if (assetRef.children == null || assetRef.children.Count == 0)
                {
                    continue;
                }

                for (int i = assetRef.children.Count - 1; i >= 0; i--)
                {
                    GameObject go = assetRef.children[i];

                    if (go == null)
                    {
                        assetRef.children.RemoveAt(i);
                    }
                }

                // 如果这个资源 assetRef 已经没有被任何 GameObject 所依赖了，那么次 assetRef 就可以卸载了

                if (assetRef.children.Count == 0)
                {
                    assetRef.asset = null;

                    Resources.UnloadUnusedAssets();

                    // 对于 assetRef 所属的这个 bundle ，解除关系

                    assetRef.bundleRef.children.Remove(assetRef);

                    if (assetRef.bundleRef.children.Count == 0)
                    {
                        assetRef.bundleRef.bundle.Unload(true);
                    }

                    // 对于 assetRef 所依赖的那些 bundle 列表，解除关系

                    foreach (BundleRef bundleRef in assetRef.dependencies)
                    {
                        bundleRef.children.Remove(assetRef);

                        if (bundleRef.children.Count == 0)
                        {
                            bundleRef.bundle.Unload(true);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 在编辑器模式下加载 AssetRef 对象
    /// </summary>
    /// <param name="moduleName">模块名字</param>
    /// <param name="assetPath">模块相对路径</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private AssetRef LoadAssetRef_Editor<T>(string moduleName, string assetPath) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(assetPath))
        {
            return null;
        }

        AssetRef assetRef = new AssetRef(null);

        assetRef.asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

        return assetRef;
#else
        return null;
#endif
    }

    /// <summary>
    /// 在运行时模式下加载 AssetRef 对象
    /// </summary>
    /// <param name="moduleName">模块名字</param>
    /// <param name="assetPath">模块相对路径</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private AssetRef LoadAssetRef_Runtime<T>(string moduleName, string assetPath) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return null;
        }

        // 先查找 update 路径下的容器，在查找 base 路径下的容器

        BaseOrUpdate witch = BaseOrUpdate.Update;

        Hashtable module2AssetRef;

        if (base2Assets.TryGetValue(moduleName, out module2AssetRef) == false)
        {
            witch = BaseOrUpdate.Base;

            if (base2Assets.TryGetValue(moduleName, out module2AssetRef) == false)
            {
                Debug.LogError("未找到资源对应的模块，moduleName " + moduleName + " assetPath " + assetPath);

                return null;
            }
        }

        AssetRef assetRef = module2AssetRef[assetPath] as AssetRef;

        if (assetRef == null)
        {
            Debug.LogError("未找到资源：moduleName " + moduleName + " assetPath " + assetPath);

            return null;
        }

        if (assetRef.asset != null)
        {
            return assetRef;
        }

        // 1. 处理 assetRef 依赖的 BundleRef 列表

        foreach (BundleRef oneBundleRef in assetRef.dependencies)
        {
            if (oneBundleRef.bundle == null)
            {
                oneBundleRef.bundle = AssetBundle.LoadFromFile(
                    BundlePath(witch, moduleName, oneBundleRef.bundleInfo.bundle_name));
            }

            if (oneBundleRef.children == null)
            {
                oneBundleRef.children = new List<AssetRef>();
            }

            oneBundleRef.children.Add(assetRef);
        }

        // 2. 处理 assetRef 属于的那个 BundleRef 对象

        BundleRef bundleRef = assetRef.bundleRef;

        if (bundleRef.bundle == null)
        {
            bundleRef.bundle = AssetBundle.LoadFromFile(
                BundlePath(witch, moduleName, bundleRef.bundleInfo.bundle_name));
        }

        if (bundleRef.children == null)
        {
            bundleRef.children = new List<AssetRef>();
        }

        bundleRef.children.Add(assetRef);

        // 3. 从 bundle 中提取 asset

        assetRef.asset = assetRef.bundleRef.bundle.LoadAsset<T>(assetRef.assetInfo.asset_path);

        if (typeof(T) == typeof(GameObject) && assetRef.assetInfo.asset_path.EndsWith(".prefab"))
        {
            assetRef.isGameObject = true;
        }
        else
        {
            assetRef.isGameObject = false;
        }

        return assetRef;
    }

    /// <summary>
    /// 工具函数 根据模块名字和 bundle 名字，返回其实际资源路径
    /// </summary>
    /// <param name="baseOrUpdate"></param>
    /// <param name="moduleName"></param>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    private string BundlePath(BaseOrUpdate baseOrUpdate, string moduleName, string bundleName)
    {
        if (baseOrUpdate == BaseOrUpdate.Update)
        {
            return Application.persistentDataPath + "/Bundles/" + moduleName + "/" + bundleName;
        }
        else
        {
            return Application.streamingAssetsPath + "/" + moduleName + "/" + bundleName;
        }
    }

    /// <summary>
    /// 平台对应的只读路径下的资源
    /// Key 模块名字
    /// Value 模块所有的资源
    /// </summary>
    public Dictionary<string, Hashtable> base2Assets;

    /// <summary>
    /// 平台对应的可读可写路径
    /// </summary>
    public Dictionary<string, Hashtable> update2Assets;

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
    /// <param name="baseOrUpdate"></param>
    /// <param name="moduleName"></param>
    /// <param name="bundleConfigName"></param>
    /// <returns></returns>
    public async Task<ModuleABConfig> LoadAssetBundleConfig(BaseOrUpdate baseOrUpdate, string moduleName, string bundleConfigName)
    {
        string url = BundlePath(baseOrUpdate, moduleName, bundleConfigName);

        UnityWebRequest request = UnityWebRequest.Get(url);

        await request.SendWebRequest();

        if (string.IsNullOrEmpty(request.error) == true)
        {
            return JsonMapper.ToObject<ModuleABConfig>(request.downloadHandler.text);
        }

        return null;
    }

}
