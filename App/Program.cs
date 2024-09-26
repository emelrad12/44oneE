using System.Runtime.InteropServices;

class LuaJITExample
{
    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr luaL_newstate();

    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    public static extern void lua_close(IntPtr luaState);

    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    public static extern int luaL_loadstring(IntPtr luaState, string script);

    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    public static extern int lua_pcall(IntPtr luaState, int nargs, int nresults, int errfunc);

    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    public static extern void lua_getfield(IntPtr luaState, int idx, string name);

    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    public static extern double lua_tonumber(IntPtr luaState, int index);

    [DllImport("luajit-5.1", CallingConvention = CallingConvention.Cdecl)]
    public static extern void lua_pushnumber(IntPtr luaState, double number);

    static void Main(string[] args)
    {
        IntPtr luaState = luaL_newstate();
        if (luaState == IntPtr.Zero)
        {
            Console.WriteLine("Failed to initialize LuaJIT.");
            return;
        }

        string luaScript = @"
        function add(a, b)
            return a + b
        end
        ";

        if (luaL_loadstring(luaState, luaScript) != 0 || lua_pcall(luaState, 0, 0, 0) != 0)
        {
            Console.WriteLine("Error loading script.");
            lua_close(luaState);
            return;
        }

        //-10002 is a magic number for global variable stolen from the c api header.
        lua_getfield(luaState, -10002, "add");
        lua_pushnumber(luaState, 10);
        lua_pushnumber(luaState, 20);

        if (lua_pcall(luaState, 2, 1, 0) != 0)
        {
            Console.WriteLine("Error calling Lua function.");
        }
        else
        {
            double result = lua_tonumber(luaState, -1);
            Console.WriteLine("Result of add(10, 20): " + result);
        }

        lua_close(luaState);
    }
}