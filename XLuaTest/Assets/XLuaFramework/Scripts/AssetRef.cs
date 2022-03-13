using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 内存中的单个资源对象
/// </summary>
public class AssetRef
{
    /// <summary>
    /// 这个资源的配置信息
    /// </summary>
    public AssetInfo assetInfo;

    /// <summary>
    /// 这个资源所属的 BundleRef 对象
    /// </summary>
    public BundleRef bundleRef;

    /// <summary>
    /// 这个资源所依赖的 BundleRef 对象列表
    /// </summary>
    public BundleRef[] dependencies;

    /// <summary>
    /// 从 bundle 文件中提取出来的资源对象
    /// </summary>
    public Object asset;

    /// <summary>
    /// 这个资源是否是 prefab
    /// </summary>
    public bool isGameObject;

    /// <summary>
    /// 这个 AssetRef 对象被哪些 GameObject 依赖
    /// </summary>
    public List<GameObject> children;

    /// <summary>
    /// AssetRef 对象的构造函数
    /// </summary>
    /// <param name="assetInfo"></param>
    public AssetRef(AssetInfo assetInfo)
    {
        this.assetInfo = assetInfo;
    }
}
