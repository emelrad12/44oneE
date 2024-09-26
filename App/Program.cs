using System;
using System.Runtime.InteropServices;

class LuaJITExample
{
    private const int LUA_GLOBALSINDEX = -10002;

    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr luaL_newstate();

    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    private static extern void lua_close(IntPtr luaState);

    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    private static extern int luaL_loadstring(IntPtr luaState, string script);

    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    private static extern int lua_pcall(IntPtr luaState, int nargs, int nresults, int errfunc);

    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    private static extern void luaL_openlibs(IntPtr luaState);

    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    public static extern long lua_tointeger(IntPtr luaState, int index);

    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    public static extern void lua_getfield(IntPtr luaState, int idx, string name);

    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    public static extern double lua_tonumber(IntPtr luaState, int index);

    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    public static extern void lua_pushnumber(IntPtr luaState, double number);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void LuaCallback(IntPtr x);

    private delegate void ForLuaCallback(int x);

    private static LuaCallback luaCallbackDelegate;
    private static IntPtr luaState;

    private static void SomeHostFunction(int x)
    {
        Console.WriteLine("Lua called host with : " + x);
    }

    static void Main()
    {
        luaState = luaL_newstate();
        luaL_openlibs(luaState);
        var luaScript = @"
            local ffi = require('ffi')
            ffi.cdef[[
                typedef void (*callback_t)(void(*x)(int));
            ]]
            function lua_callback(some_host_function)
                print('host called lua ptr')
                some_host_function(123456)
            end
            function get_ptr()
                local cb_ptr = ffi.cast('callback_t', lua_callback)
                cb_ptr = ffi.cast(""void(*)()"", cb_ptr)
                cb_ptr = ffi.cast('long long', cb_ptr)
                return tonumber(cb_ptr)
            end
        ";
        luaL_loadstring(luaState, luaScript);
        lua_pcall(luaState, 0, 0, 0);
        lua_getfield(luaState, -10002, "get_ptr");
        lua_pushnumber(luaState, 10);
        lua_pushnumber(luaState, 20);
        var res = lua_pcall(luaState, 2, 1, 0);
        if (res != 0) throw new("Error calling lua function");
        var result2 = lua_tointeger(luaState, -1);
        var luaCallbackPtr = Marshal.GetFunctionPointerForDelegate((ForLuaCallback)SomeHostFunction);
        Marshal.GetDelegateForFunctionPointer<LuaCallback>((IntPtr)result2)(luaCallbackPtr);
        lua_close(luaState);
    }
}