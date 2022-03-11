using UnityEngine;

public class Main : MonoBehaviour
{
    public void Awake()
    {
        InitGlobal();

        // 启动模块
        ModuleConfig launchModule = new ModuleConfig()
        {
            moduleName = "Launch",
            moduleVersion = "20220311143900",
            moduleUrl = "http://192.168.0.7:8000"
        };

        ModuleManager.Instance.Load(launchModule, (success) =>
        {
            // 在这里把代码控制权交给Lua
            Debug.Log("Lua 代码开始...");
        });
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
    /// 主Mono对象
    /// </summary>
    public static Main Instance;
}
