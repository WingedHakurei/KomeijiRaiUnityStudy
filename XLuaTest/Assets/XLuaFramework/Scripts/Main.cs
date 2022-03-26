using System.IO;
using UnityEngine;
using XLua;
using static XLua.LuaEnv;

public class Main : MonoBehaviour
{
    public async void Awake()
    {
        InitGlobal();

        InitCustomLoaders();

        // 启动模块
        ModuleConfig launchModule = new ModuleConfig()
        {
            moduleName = "Launch",
            moduleVersion = "20220311143900",
            moduleUrl = "http://192.168.17.1:8000/chfs/shared"
        };

        bool result = await ModuleManager.Instance.Load(launchModule);

        if (result == true)
        {
            Debug.Log("Lua 代码开始...");

            gameObject.AddComponent<MonoProxy>().BindScript("Launch", "Main");
        }
    }

    private void Update()
    {
        // 执行卸载策略

        AssetLoader.Instance.Unload(AssetLoader.Instance.base2Assets);
    }

    /// <summary>
    /// 初始化全局变量
    /// </summary>
    private void InitGlobal()
    {
        Instance = this;

        GlobalConfig.HotUpdate = false;

        GlobalConfig.BundleMode = false;

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 整个工程共享一个 LuaEnv 对象
    /// </summary>
    /// <returns></returns>
    public LuaEnv luaEnv { get; } = new LuaEnv();

    /// <summary>
    /// 初始化自定义 Lua 加载器
    /// </summary>
    private void InitCustomLoaders()
    {
        DirectoryInfo baseDir = new DirectoryInfo(Application.dataPath + "/GAssets");

        // 遍历所有模块

        DirectoryInfo[] dirs = baseDir.GetDirectories();

        foreach (DirectoryInfo moduleDir in dirs)
        {
            string moduleName = moduleDir.Name;

            CustomLoader loader = (ref string scriptPath) =>
            {
                string assetPath = "Assets/GAssets/" + moduleName + "/Src/" + scriptPath.Trim() + ".lua.txt";

                TextAsset asset = AssetLoader.Instance.CreateAsset<TextAsset>("Launch", assetPath, Main.Instance.gameObject);

                if (asset != null)
                {
                    string scriptString = asset.text;

                    byte[] result = System.Text.Encoding.UTF8.GetBytes(scriptString);

                    return result;
                }

                return null;
            };

            luaEnv.AddLoader(loader);
        }
    }

    /// <summary>
    /// 主Mono对象
    /// </summary>
    public static Main Instance;
}
