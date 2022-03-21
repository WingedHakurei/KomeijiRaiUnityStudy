using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
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

            asset2Dependencies.Clear();

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

            SaveModuleABConfig(moduleName);

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

    /// <summary>
    /// 将一个模块的资源依赖关系数据保存成json格式的文件
    /// </summary>
    /// <param name="moduleName">模块名字</param>
    private static void SaveModuleABConfig(string moduleName)
    {
        ModuleABConfig moduleABConfig = new ModuleABConfig(asset2bundle.Count);

        // 记录AB包信息

        foreach (AssetBundleBuild build in assetBundleBuildList)
        {
            BundleInfo bundleInfo = new BundleInfo();

            bundleInfo.bundle_name = build.assetBundleName;

            bundleInfo.assets = new List<string>();

            foreach (string asset in build.assetNames)
            {
                bundleInfo.assets.Add(asset);
            }

            // 计算一个bundle文件的CRC散列码

            string abFilePath = abOutputPath + "/" + moduleName + "/" + bundleInfo.bundle_name;

            using (FileStream stream = File.OpenRead(abFilePath))
            {
                bundleInfo.crc = AssetUtility.GetCRC32Hash(stream);

                // 计算一个bundle文件的大小

                bundleInfo.size = (int)stream.Length;
            }

            moduleABConfig.AddBundle(bundleInfo.bundle_name, bundleInfo);
        }

        // 记录每个资源的依赖关系

        int assetIndex = 0;

        foreach (var item in asset2bundle)
        {
            AssetInfo assetInfo = new AssetInfo();
            assetInfo.asset_path = item.Key;
            assetInfo.bundle_name = item.Value;
            assetInfo.dependencies = new List<string>();

            bool result = asset2Dependencies.TryGetValue(item.Key, out List<string> dependencies);

            if (result == true)
            {
                for (int i = 0; i < dependencies.Count; i++)
                {
                    string bundleName = dependencies[i];

                    assetInfo.dependencies.Add(bundleName);
                }
            }

            moduleABConfig.AddAsset(assetIndex, assetInfo);

            assetIndex++;
        }

        // 开始写入Json文件

        string moduleConfigName = moduleName.ToLower() + ".json";

        string jsonPath = abOutputPath + "/" + moduleName + "/" + moduleConfigName;

        if (File.Exists(jsonPath) == true)
        {
            File.Delete(jsonPath);
        }

        File.Create(jsonPath).Dispose();

        string jsonData = LitJson.JsonMapper.ToJson(moduleABConfig);

        File.WriteAllText(jsonPath, ConvertJsonString(jsonData));
    }

    /// <summary>
    /// 格式化json
    /// </summary>
    /// <param name="str">输入json字符串</param>
    /// <returns>返回格式化后的字符串</returns>
    private static string ConvertJsonString(string str)
    {
        JsonSerializer serializer = new JsonSerializer();

        TextReader tr = new StringReader(str);

        JsonTextReader jtr = new JsonTextReader(tr);

        object obj = serializer.Deserialize(jtr);
        if (obj != null)
        {
            StringWriter textWriter = new StringWriter();

            JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
            {
                Formatting = Formatting.Indented,

                Indentation = 4,

                IndentChar = ' '
            };

            serializer.Serialize(jsonWriter, obj);

            return textWriter.ToString();
        }
        else
        {
            return str;
        }
    }
}
