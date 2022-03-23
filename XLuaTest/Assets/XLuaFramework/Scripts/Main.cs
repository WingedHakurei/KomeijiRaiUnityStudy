using UnityEngine;

public class Main : MonoBehaviour
{
    public async void Awake()
    {
        InitGlobal();

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

            AssetLoader.Instance.Clone("Launch", "Assets/GAssets/Launch/Sphere.prefab");

            GameObject komeijiRai = AssetLoader.Instance.Clone("Launch", "Assets/GAssets/Launch/KomeijiRai.prefab");

            komeijiRai.GetComponent<SpriteRenderer>().sprite =
                AssetLoader.Instance.CreateAsset<Sprite>(
                    "Launch",
                    "Assets/GAssets/Launch/Sprite/KomeijiRai.png",
                    komeijiRai);
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

        GlobalConfig.HotUpdate = true;

        GlobalConfig.BundleMode = true;

        DontDestroyOnLoad(gameObject);
    }
    /// <summary>
    /// 主Mono对象
    /// </summary>
    public static Main Instance;
}
