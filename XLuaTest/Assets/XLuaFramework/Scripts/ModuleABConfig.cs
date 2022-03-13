using System.Collections.Generic;
/// <summary>
/// 一个Bundle数据 用于序列化为json文件
/// </summary>
public class BundleInfo
{
    /// <summary>
    /// 这个bundle的名字
    /// </summary>
    public string bundle_name;

    /// <summary>
    /// 这个bundle的crc散列码
    /// </summary>
    public string crc;

    /// <summary>
    /// 这个bundle所包含的资源的路径列表
    /// </summary>
    public List<string> assets;
}

/// <summary>
/// 一个Asset数据 用于序列化为json文件
/// </summary>
public class AssetInfo
{
    /// <summary>
    /// 这个资源的相对路径
    /// </summary>
    public string asset_path;

    /// <summary>
    /// 这个资源所属的AssetBundle的名字
    /// </summary>
    public string bundle_name;

    /// <summary>
    /// 这个资源所依赖的AssetBundle列表的名字
    /// </summary>
    public List<string> dependencies;
}

/// <summary>
/// ModuleABConfig对象 对应 整个单个模块的json文件
/// </summary>
public class ModuleABConfig
{
    public ModuleABConfig(int assetCount)
    {
        BundleArray = new Dictionary<string, BundleInfo>();
        AssetArray = new AssetInfo[assetCount];
    }

    public ModuleABConfig() { }

    /// <summary>
    /// Key：AssetBundle的名字
    /// </summary>
    public Dictionary<string, BundleInfo> BundleArray;

    /// <summary>
    /// asset 数组
    /// </summary>
    public AssetInfo[] AssetArray;

    /// <summary>
    /// 新增一个bundle记录
    /// </summary>
    /// <param name="bundleName">bundle的id</param>
    /// <param name="bundleInfo">bundle的对象</param>
    public void AddBundle(string bundleName, BundleInfo bundleInfo)
    {
        BundleArray[bundleName] = bundleInfo;
    }

    /// <summary>
    /// 新增一个资源记录
    /// </summary>
    /// <param name="index"></param>
    /// <param name="assetInfo"></param>
    public void AddAsset(int index, AssetInfo assetInfo)
    {
        AssetArray[index] = assetInfo;
    }
}