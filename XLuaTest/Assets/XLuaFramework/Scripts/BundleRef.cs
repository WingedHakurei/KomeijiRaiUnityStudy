using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 在内存中的一个Bundle对象
/// </summary>
public class BundleRef
{
    /// <summary>
    /// 这个 bundle 的静态配置信息
    /// </summary>
    public BundleInfo bundleInfo;

    /// <summary>
    /// 记录这个 BundleRef 对应的AB文件需要从哪里加载
    /// </summary>
    public BaseOrUpdate witch;

    /// <summary>
    /// 加载到内存的 bundle 对象
    /// </summary>
    public AssetBundle bundle;

    /// <summary>
    /// 这些 BundleRef 对象被哪些 AssetRef 对象依赖
    /// </summary>
    public List<AssetRef> children;

    /// <summary>
    /// BundleRef 的构造函数
    /// </summary>
    /// <param name="bundleInfo"></param>
    public BundleRef(BundleInfo bundleInfo, BaseOrUpdate witch)
    {
        this.bundleInfo = bundleInfo;

        this.witch = witch;
    }
}
