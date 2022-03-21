using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 下载器 工具类
/// </summary>
public class Downloader : Singleton<Downloader>
{
    /// <summary>
    /// 根据模块的配置，下载对应的模块
    /// </summary>
    /// <param name="moduleConfig"></param>
    /// <returns></returns>
    public async Task Download(ModuleConfig moduleConfig)
    {
        // 用来存放热更下来的资源的本地路径

        string updatePath = GetUpdatePath(moduleConfig.moduleName);

        // 远程服务器上这个模块的AB资源配置文件的URL

        string configURL = GetServerURL(moduleConfig, moduleConfig.moduleName.ToLower() + ".json");

        UnityWebRequest request = UnityWebRequest.Get(configURL);

        request.downloadHandler = new DownloadHandlerFile(
            $"{updatePath}/{moduleConfig.moduleName.ToLower()}_temp.json");

        Debug.Log("下载到本地路径：" + updatePath);

        await request.SendWebRequest();

        if (string.IsNullOrEmpty(request.error) == false)
        {
            Debug.LogWarning($"下载模块{moduleConfig.moduleName}的AB配置文件：{request.error}");

            bool result = await ShowMessageBox("网络异常，请检查网络后点击 继续下载", "继续下载", "退出游戏");

            if (result == false)
            {
                Application.Quit();

                return;
            }

            await Download(moduleConfig);

            return;
        }

        Tuple<List<BundleInfo>, BundleInfo[]> tuple = await GetDownloadList(moduleConfig.moduleName);

        List<BundleInfo> downloadList = tuple.Item1;

        BundleInfo[] removeList = tuple.Item2;

        long downloadSize = CalculateSize(downloadList);

        if (downloadSize == 0L)
        {
            return;
        }

        bool boxResult = await ShowMessageBox(moduleConfig, downloadSize);

        if (boxResult == false)
        {
            Application.Quit();

            return;
        }

        await ExecuteDownload(moduleConfig, downloadList);

        Clear(moduleConfig, removeList);

        return;
    }

    /// <summary>
    /// 模块热更新完成后的善后工作
    /// </summary>
    /// <param name="moduleConfig"></param>
    /// <param name="removeList"></param>
    private void Clear(ModuleConfig moduleConfig, BundleInfo[] removeList)
    {
        string moduleName = moduleConfig.moduleName;

        string updatePath = GetUpdatePath(moduleName);

        for (int i = removeList.Length - 1; i >= 0; i--)
        {
            BundleInfo bundleInfo = removeList[i];

            string filePath = $"{updatePath}/{bundleInfo.bundle_name}";

            File.Delete(filePath);
        }

        // 删除旧的配置文件

        string oldFile = $"{updatePath}/{moduleName.ToLower()}.json";

        if (File.Exists(oldFile))
        {
            File.Delete(oldFile);
        }

        // 用新的配置文件替代之

        string newFile = $"{updatePath}/{moduleName.ToLower()}_temp.json";

        File.Move(newFile, oldFile);
    }

    /// <summary>
    /// 计算需要下载的资源大小 单位是字节
    /// </summary>
    /// <param name="bundleList"></param>
    /// <returns></returns>
    private long CalculateSize(List<BundleInfo> bundleList)
    {
        long totalSize = 0L;

        foreach (BundleInfo bundleInfo in bundleList)
        {
            totalSize += bundleInfo.size;
        }

        return totalSize;
    }

    /// <summary>
    /// 弹出对话框
    /// </summary>
    /// <param name="moduleConfig"></param>
    /// <param name="totalSize"></param>
    /// <returns></returns>
    private async Task<bool> ShowMessageBox(ModuleConfig moduleConfig, long totalSize)
    {
        string downloadSize = SizeToString(totalSize);

        string messageInfo = $"发现新版本，版本号为：{moduleConfig.moduleVersion}\n需要下载热更包，大小为：{downloadSize}";

        MessageBox messageBox = new MessageBox(messageInfo, "开始下载", "退出游戏");

        MessageBox.BoxResult result = await messageBox.GetReplyAsync();

        messageBox.Close();

        if (result == MessageBox.BoxResult.First)
        {
            return true;
        }

        return false;
    }

    private async Task<bool> ShowMessageBox(string messageInfo, string first, string second)
    {
        MessageBox messageBox = new MessageBox(messageInfo, first, second);

        MessageBox.BoxResult result = await messageBox.GetReplyAsync();

        messageBox.Close();

        if (result == MessageBox.BoxResult.First)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 工具函数 把字节数转换成字符串形式
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    string SizeToString(long size)
    {
        string sizeStr = "";

        if (size >= 1024 * 1024)
        {
            long m = size / (1024 * 1024);

            size = size % (1024 * 1024);

            sizeStr += $"{m}[M]";
        }

        if (size >= 1024)
        {
            long k = size / 1024;

            size = size % 1024;

            sizeStr += $"{k}[K]";
        }

        long b = size;

        sizeStr += $"{b}[B]";

        return sizeStr;
    }

    /// <summary>
    /// 客户端给定模块的热更资源存放地址
    /// </summary>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    private string GetUpdatePath(string moduleName)
    {
        return Application.persistentDataPath + "/Bundles/" + moduleName;
    }

    /// <summary>
    /// 返回 给定模块的给定文件在服务器端的完整URL
    /// </summary>
    /// <param name="moduleConfig"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private string GetServerURL(ModuleConfig moduleConfig, string fileName)
    {
#if UNITY_ANDROID
        return $"{moduleConfig.DownloadURL}/Android/{fileName}";
#elif UNITY_IOS
        return $"{moduleConfig.DownloadURL}/iOS/{fileName}";
#else
        return $"{moduleConfig.DownloadURL}/StandaloneWindows64/{fileName}";
#endif
    }

    /// <summary>
    /// 对于给定模块，返回其所有需要下载的 BundleInfo 组成的 List
    /// </summary>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    private async Task<Tuple<List<BundleInfo>, BundleInfo[]>> GetDownloadList(string moduleName)
    {
        ModuleABConfig serverConfig = await AssetLoader.Instance.LoadAssetBundleConfig(
            BaseOrUpdate.Update,
            moduleName,
            moduleName.ToLower() + "_temp.json");

        if (serverConfig == null)
        {
            return null;
        }

        ModuleABConfig localConfig = await AssetLoader.Instance.LoadAssetBundleConfig(
            BaseOrUpdate.Update,
            moduleName,
            moduleName.ToLower() + ".json");

        // 注意，这里不用判断 localConfig 是否存在，本地的 localConfig 确实可能不存在
        // 比如在此模块第一次热更新之前，本地 update 路径下啥都没有

        return CalculateDiff(moduleName, localConfig, serverConfig);
    }

    /// <summary>
    /// 通过两个 AB 资源配置文件，对比出有差异的 Bundle
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="localConfig"></param>
    /// <param name="serverConfig"></param>
    /// <returns></returns>
    private Tuple<List<BundleInfo>, BundleInfo[]> CalculateDiff(string moduleName, ModuleABConfig localConfig, ModuleABConfig serverConfig)
    {

        List<BundleInfo> bundleList = new List<BundleInfo>();

        Dictionary<string, BundleInfo> localBundleDic = new Dictionary<string, BundleInfo>();

        if (localConfig != null)
        {
            foreach (BundleInfo bundleInfo in localConfig.BundleArray.Values)
            {
                string uniqueId = $"{bundleInfo.bundle_name} | {bundleInfo.crc}";

                localBundleDic.Add(uniqueId, bundleInfo);
            }
        }

        // 找到那些差异的 bundle 文件，放到 bundleList 容器中
        // 对于那些遗留在本地的无用的 bundle 文件，把它过滤在 localBundleDic 容器里

        foreach (BundleInfo bundleInfo in serverConfig.BundleArray.Values)
        {
            string uniqueId = $"{bundleInfo.bundle_name} | {bundleInfo.crc}";

            if (localBundleDic.ContainsKey(uniqueId) == false)
            {
                bundleList.Add(bundleInfo);
            }
            else
            {
                localBundleDic.Remove(uniqueId);
            }
        }

        BundleInfo[] removeList = localBundleDic.Values.ToArray();

        return new Tuple<List<BundleInfo>, BundleInfo[]>(bundleList, removeList);
    }

    /// <summary>
    /// 执行下载行为
    /// </summary>
    /// <param name="moduleConfig"></param>
    /// <param name="bundleList"></param>
    /// <returns>返回的 List 包含的是还未下载的 Bundle</returns>
    private async Task ExecuteDownload(ModuleConfig moduleConfig, List<BundleInfo> bundleList)
    {
        while (bundleList.Count > 0)
        {
            BundleInfo bundleInfo = bundleList[0];

            UnityWebRequest request = UnityWebRequest.Get(GetServerURL(moduleConfig, bundleInfo.bundle_name));

            string updatePath = GetUpdatePath(moduleConfig.moduleName);

            request.downloadHandler = new DownloadHandlerFile(
                $"{updatePath}/{bundleInfo.bundle_name}");

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("下载资源：" + bundleInfo.bundle_name + " 成功");

                bundleList.RemoveAt(0);
            }
            else
            {
                break;
            }
        }

        if (bundleList.Count > 0)
        {
            bool result = await ShowMessageBox("网络异常，请检查网络后点击 继续下载", "继续下载", "退出游戏");

            if (result == false)
            {
                Application.Quit();

                return;
            }

            await ExecuteDownload(moduleConfig, bundleList);

            return;
        }
    }

}
