using System;
using UniRx;
using UnityEngine;
using XLua;

/// <summary>
/// MonoBehaviour 的代理类
/// </summary>
public class MonoProxy : MonoBehaviour
{
    /// <summary>
    /// 这个 MonoProxy 对象所绑定的 lua 脚本对象
    /// </summary>
    public LuaTable luaTable;

    private Action<LuaTable> luaStart;

    private Action<LuaTable> luaOnDestroy;

    /// <summary>
    /// 绑定对应的脚本
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="scriptPath">输入的 lua 脚本文件的相对路径</param>
    public LuaTable BindScript(string moduleName, string scriptPath)
    {
        Main.Instance.luaEnv.DoString("require '" + scriptPath + "'");

        luaTable = Main.Instance.luaEnv.Global.Get<LuaTable>(scriptPath);

        // 给这个 luaTable 对象添加一个字段指向这个 c# 的 MonoProxy 对象

        luaTable.Set("MonoProxy", this);

        // 补一个 Awake 方法调用

        Action<LuaTable> luaAwake = luaTable.Get<Action<LuaTable>>("Awake");

        luaAwake?.Invoke(luaTable);

        // 获取 lua 脚本的成员方法

        luaTable.Get("Start", out luaStart);

        luaTable.Get("OnDestroy", out luaOnDestroy);

        return luaTable;
    }

    private void Start()
    {
        luaStart?.Invoke(luaTable);
    }

    /// <summary>
    /// 给 MonoProxy 对应的 Lua 脚本绑定一个 Update 方法！按需使用！
    /// </summary>
    /// <param name="action"></param>
    public void BindUpdate(Action action)
    {
        Observable.EveryUpdate().Subscribe(_ => { action(); }).AddTo(this);
    }

    private void OnDestroy()
    {
        luaOnDestroy?.Invoke(luaTable);
    }

}
