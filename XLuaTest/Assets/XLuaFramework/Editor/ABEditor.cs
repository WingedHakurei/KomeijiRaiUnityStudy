using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 生成AssetBundle的编辑器工具
/// </summary>
public class ABEditor : MonoBehaviour
{
    /// <summary>
    /// 热更资源根目录
    /// </summary>
    public static string rootPath = Application.dataPath + "/GAssets";

    /// <summary>
    /// 所有需要打包的AB包信息：一个AssetBundle文件对应了一个AssetBundleBuild对象
    /// </summary>
    public static List<AssetBundleBuild> assetBundleBuildList = new List<AssetBundleBuild>();

    /// <summary>
    /// AB包文件的输出路径
    /// </summary>
    public static string abOutputPath = Application.streamingAssetsPath;

    /// <summary>
    /// 记录哪个asset资源属于哪个AB包文件
    /// </summary>
    public static Dictionary<string, string> asset2bundle = new Dictionary<string, string>();

    /// <summary>
    /// 记录每个asset资源以来的AB包文件列表
    /// </summary>
    public static Dictionary<string, List<string>> asset2Dependencies = new Dictionary<string, List<string>>();

    /// <summary>
    /// 打包AssetBundle资源
    /// </summary>
    [MenuItem("ABEditor/BuildAssetBundle")]
    public static void BuildAssetBundle()
    {
        Debug.Log("开始--->>>生成所有模块的AB包！");

        if (Directory.Exists(abOutputPath) == true)
        {
            Directory.Delete(abOutputPath, true);
        }

        // 遍历所有模块，针对所有模块都分别打包

        DirectoryInfo rootDir = new DirectoryInfo(rootPath);

        DirectoryInfo[] dirs = rootDir.GetDirectories();

        foreach (DirectoryInfo moduleDir in dirs)
        {
            string moduleName = moduleDir.Name;

            assetBundleBuildList.Clear();

            asset2bundle.Clear();

            // 开始给这个模块生成AB包文件

            ScanChildDirectories(moduleDir);

            AssetDatabase.Refresh();

            string moduleOutputPath = abOutputPath + "/" + moduleName;

            if (Directory.Exists(moduleOutputPath) == true)
                Directory.Delete(moduleOutputPath, true);

            Directory.CreateDirectory(moduleOutputPath);

            // BuildAssetBundleOptions.None：使用LZMA算法压缩，包小加载慢；一旦被解压，会使用LZ4重新压缩
            // BuildAssetBundleOptions.UncompressedAssetBundle：不压缩，包大加载快
            // BuildAssetBundleOptions.ChunkBasedCompression：使用LZ4压缩，均衡且可加载指定资源
            // EditorUserBuildSettings：目标平台
            BuildPipeline.BuildAssetBundles
            (
                moduleOutputPath,
                assetBundleBuildList.ToArray(),
                BuildAssetBundleOptions.None,
                EditorUserBuildSettings.activeBuildTarget
            );

            CalculateDependencies();

            AssetDatabase.Refresh();
        }

        Debug.Log("结束--->>>生成所有模块的AB包！");
    }

    /// <summary>
    /// 根据指定的文件夹
    /// 1. 将这个文件夹下的所有一级子文件打成一个AssetBundle
    /// 2. 并且递归遍历这个文件夹下的所有子文件夹
    /// </summary>
    /// <param name="directoryInfo"></param>
    static void ScanChildDirectories(DirectoryInfo directoryInfo)
    {

        // 收集当前路径下的文件 把它们打成一个AB包

        ScanCurDirectory(directoryInfo);

        // 遍历当前路径下的子文件夹

        DirectoryInfo[] dirs = directoryInfo.GetDirectories();

        foreach (DirectoryInfo info in dirs)
        {
            ScanChildDirectories(info);
        }
    }

    /// <summary>
    /// 遍历当前路径下的文件 把它们打成一个AB包
    /// </summary>
    /// <param name="directoryInfo"></param>
    static void ScanCurDirectory(DirectoryInfo directoryInfo)
    {
        List<string> assetNames = new List<string>();

        FileInfo[] fileInfoList = directoryInfo.GetFiles();

        foreach (FileInfo fileInfo in fileInfoList)
        {
            if (fileInfo.FullName.EndsWith(".meta"))
                continue;

            // assetName的格式类似 "Assets/GAssets/Launch/Sphere.prefab"

            string assetName = fileInfo.FullName.Substring(Application.dataPath.Length - "Assets".Length).Replace('\\', '/');

            assetNames.Add(assetName);
        }

        if (assetNames.Count > 0)
        {
            // 格式类似 gassets_Launch

            string assetbundleName = directoryInfo.FullName.Substring(Application.dataPath.Length + 1).Replace('\\', '_').ToLower();

            AssetBundleBuild build = new AssetBundleBuild();

            build.assetBundleName = assetbundleName;

            build.assetNames = new string[assetNames.Count];

            for (int i = 0; i < assetNames.Count; i++)
            {
                build.assetNames[i] = assetNames[i];

                // 记录单个资源属于哪个bundle文件

                asset2bundle.Add(assetNames[i], assetbundleName);
            }

            assetBundleBuildList.Add(build);
        }
    }

    /// <summary>
    /// 计算每个资源所依赖的AB包文件列表
    /// </summary>
    public static void CalculateDependencies()
    {
        foreach (string asset in asset2bundle.Keys)
        {
            // 这个资源自己所在的bundle
            string assetBundle = asset2bundle[asset];

            string[] dependencies = AssetDatabase.GetDependencies(asset);

            List<string> assetList = new List<string>();

            if (dependencies != null && dependencies.Length > 0)
            {
                foreach (string oneAsset in dependencies)
                {
                    if (oneAsset == asset || oneAsset.EndsWith(".cs"))
                        continue;

                    assetList.Add(oneAsset);
                }
            }

            if (assetList.Count > 0)
            {
                List<string> abList = new List<string>();

                foreach (var oneAsset in assetList)
                {
                    bool result = asset2bundle.TryGetValue(oneAsset, out string bundle);

                    if (result == true && bundle != assetBundle)
                    {
                        abList.Add(bundle);
                    }
                }

                asset2Dependencies.Add(asset, abList);
            }

        }
    }

}
