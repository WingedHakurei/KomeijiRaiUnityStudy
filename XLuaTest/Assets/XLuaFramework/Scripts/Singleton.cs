/// <summary>
/// 不继承Mono的单例模式
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : new()
{
    public static T Instance
    {
        get => instance ?? (instance = new T());
    }
    private static T instance;
}
