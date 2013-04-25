using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace ComicDown.UI.Core.Bolt
{
	/// <summary>
	/// Lua Inner Index
	/// </summary>
	public enum LuaInnerIndex
	{
		LUA_REGISTRYINDEX = -10000,
		LUA_ENVIRONINDEX = -10001,
		LUA_GLOBALSINDEX = -10002,
	}
	
	/// <summary>
	/// Lua Baic Types
	/// </summary>
	public enum LuaTypes
	{
		LUA_TNIL = 0,
		LUA_TBOOLEAN = 1,
		LUA_TLIGHTUSERDATA = 2,
		LUA_TNUMBER = 3,
		LUA_TSTRING = 4,
		LUA_TTABLE = 5,
		LUA_TFUNCTION = 6,
		LUA_TUSERDATA = 7,
		LUA_TTHREAD = 8
	}


    [StructLayout(LayoutKind.Sequential)]
    public struct XLLRTErrorHash
    {
        public UInt16 top;
        public UInt32 topsix;
        public UInt32 all;
    }
	
    [StructLayout(LayoutKind.Sequential)]
    public struct XL_LRT_ERROR_STACK
    {
        public IntPtr logs;
        public XLLRTErrorHash hash;
    }

	/// <summary>
	/// Lua C Function 
	/// </summary>
	/// <param name="L">Lua State</param>
	/// <returns></returns>
	[UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
	public delegate int LuaCFunction (IntPtr luaState);

    [UnmanagedFunctionPointerAttribute(CallingConvention.StdCall,CharSet=CharSet.Unicode)]
    public delegate int fnLuaErrorHandle(
        IntPtr luaState,
        [MarshalAs(UnmanagedType.LPWStr)]
        string pExtInfo,
        [MarshalAs(UnmanagedType.LPWStr)]
        string luaErrorString,
        IntPtr pStackInfo
    );


	/// <summary>
	/// XLLRT GetObject CallBack
	/// </summary>
	/// <param name="ud"></param>
	/// <returns></returns>
	public delegate IntPtr XLLRTFuncGetObject(IntPtr ud);

    /// <summary>
    /// XLLRT Global API
    /// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct XLLRTGlobalAPI
	{
		[MarshalAs(UnmanagedType.LPStr)]
		public String funName;
		public LuaCFunction func;
		public UInt32 permission;
	}
	
    /// <summary>
    /// XLLRT Object
    /// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct XLLRTObject
	{
		public XLLRTFuncGetObject pfnGetObject;

		public IntPtr userData;

		[MarshalAs(UnmanagedType.LPStr)]
		public String objName;

		[MarshalAs(UnmanagedType.LPStr)]
		public String className;

		public IntPtr memberFunctions;//XLLRTGlobalAPI数组

		public UInt32 permission;
	}

    /// <summary>
    /// XLLRT Class
    /// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct XLLRTClass
	{
		[MarshalAs(UnmanagedType.LPStr)]
		public String className;

		[MarshalAs(UnmanagedType.LPStr)]
		public String fahterClassName;

		public IntPtr MemberFunctions;//XLLRTGlobalAPI数组

		public UInt32 permission;
	}

    public sealed class XLRTObjectInfo
    {
        public string ClassName { get; set; }
        public string ObjectName { get; set; }
        public IntPtr UserData { get; set; }
        public XLLRTFuncGetObject GetFunction { get; set; }
        public Dictionary<string, LuaCFunction> Methods { get; set; }
    }
    public sealed class XLRTClassInfo
    {
        public string ClassName { get; set; }
        public string FatherClassName { get; set; }
        public LuaCFunction DeleteFunction { get; set; }
        public Dictionary<string, LuaCFunction> Methods { get; set; }
    }
    public sealed class XLRTMethodsInfo
    {
        public Dictionary<string, LuaCFunction> Methods { get; set; }
    }

    /// <summary>
    /// Xunlei Lua Runtime .NET Wrapper
    /// </summary>
	public sealed class XLLuaRuntime
    {
        #region Singleton 
        /// <summary>
        /// static instance of XLLuaRuntime
        /// </summary>
        private static XLLuaRuntime instance = null;

        /// <summary>
        /// static constructor
        /// </summary>
        /// <returns></returns>
        public static XLLuaRuntime Instance()
        {
            if (instance == null) {
                instance = new XLLuaRuntime();
            }
            return instance;
        }
        #endregion

        #region Set Dll Path
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);
        static XLLuaRuntime()
        {
            //SetDllDirectory(AppDomain.CurrentDomain.BaseDirectory + "libbolt");
        }
        #endregion

        #region XLLRT Chunk and LuaCall
        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int XLLRT_AddRefChunk(IntPtr hChunk);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int XLLRT_ReleaseChunk(IntPtr hChunk);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8MarshalerNoCleanUp))]
        public static extern string XLLRT_GetChunkName(IntPtr hChunk);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int XLLRT_GetChunkType(IntPtr hChunk);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int XLLRT_CreateChunk(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
            string pstrName,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
            string pCodeBuffer,
            long len,
            [In, Out]
            IntPtr pResult);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int XLLRT_CreateChunkFromFile(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
            string pstrName,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
            string path,
            [In, Out]
            IntPtr pResult);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int XLLRT_CreateChunkFromModule(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
            string pstrName,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
            string modulePath,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
            string func,
            [In, Out]
            IntPtr pResult);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int XLLRT_RunChunk(IntPtr hRunTime,IntPtr hChunk);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int XLLRT_PrepareChunk(IntPtr hRunTime,IntPtr hChunk);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int XLLRT_LuaCall(
            IntPtr luaState, 
            int arg, 
            int ret,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
            string ustr);

        #endregion

        #region XLLRT Get API
        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr XLLRT_GetEnv(IntPtr eneviromentID);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr XLLRT_GetLuaState(IntPtr hRunTime);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr XLLRT_GetRuntime(
            IntPtr hEnviroment,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
            string runtimeID);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr XLLRT_GetRuntimeFromLuaState(IntPtr luaState);
        #endregion

        #region XLLRT Register API
        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr XLLRT_RegisterGlobalObj(IntPtr env, XLLRTObject theObject);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr XLLRT_RegisterGlobalAPI(IntPtr env, XLLRTGlobalAPI theAPI);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr XLLRT_IsGlobalAPIRegistered(IntPtr env,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
            string theAPIName);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr XLLRT_RegisterClass(IntPtr ud,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
            string className, 
            IntPtr memberFunctions,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
            string fatherClassname, UInt32 permission);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int XLLRT_PushXLObject(IntPtr luaState,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
            string className, 
            IntPtr pRealObj);
        #endregion

        #region XLLRT GlobalAPI Enumerator
        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr XLLRT_BeginEnumGlobalAPI(IntPtr hEnv);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool XLLRT_GetNextGlobalAPI(IntPtr hEnum,IntPtr lpGlobalAPI);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool XLLRT_EndEnum(IntPtr hEnum);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr XLLRT_BeginEnumGlobalClass(IntPtr hEnv);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool XLLRT_GetNextGlobalClass(IntPtr hEnum, IntPtr lpGlobalClass);
        #endregion

        #region XLLRT Error
        // 重要！设置一个错误回调函数，在luacall出现脚本错误后，会回调该函数
        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern long XLLRT_ErrorHandle(fnLuaErrorHandle  pfnErrorHandle);

        // 获取当前luastate产生的最后一条错误信息
        [DllImport("XLUE.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8MarshalerNoCleanUp))]
        public static extern string XLLRT_GetLastError(IntPtr luaState);
        #endregion

        #region XLLRT DEBUG API

        #endregion

        #region Lua Push API
        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushstring(
            IntPtr luaState,
            [MarshalAs(UnmanagedType.CustomMarshaler,MarshalTypeRef = typeof(UTF8Marshaler))]
            string s);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushinteger(IntPtr luaState, int n);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushnil(IntPtr luaState);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushboolean(IntPtr luaState, int boolean);

        public static void lua_pushboolean(IntPtr luaState, bool boolean)
        {
            lua_pushboolean(luaState,boolean?1:0);
        }

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushnumber(IntPtr luaState, double n);
        #endregion

        #region Lua To API
        [DllImport("XLUE.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8MarshalerNoCleanUp))]
        public static extern string lua_tolstring(IntPtr luaState, int index,UIntPtr len);

        public static string lua_tostring(IntPtr luaState, int index)
        {
            return lua_tolstring(luaState, index,UIntPtr.Zero);
        }

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_tointeger(IntPtr luaState, int index);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_toboolean(IntPtr luaState, int index);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double lua_tonumber(IntPtr luaState, int index);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_objlen(IntPtr luaState, int index);
        #endregion

        #region Lua Table API
        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_setfield(
            IntPtr luaState, 
            int index,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
            string key);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawgeti(IntPtr luaState, int index, int n);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawseti(IntPtr luaState, int idx, int n);


        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_createtable(IntPtr luaState, int narr, int nrec);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_settable(IntPtr luaState, int idx);


        public static void lua_newtable(IntPtr luaState)
        {
            lua_createtable(luaState, 0, 0);
        }
        #endregion

        #region Lua Other Stack Operation
        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr luaL_checkudata(IntPtr luaState, int ud, string tname);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_type(IntPtr luaState, int index);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_ref(IntPtr luaState, int t);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_unref(IntPtr luaState, int t, int n);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_gettop(IntPtr luaState);

        [DllImport("XLUE.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_settop(IntPtr luaState, int newTop);
        #endregion

        #region Lua LightUserData
        [DllImport("XLUE.dll",CallingConvention=CallingConvention.Cdecl)]
        public static extern void  lua_pushlightuserdata(IntPtr L, IntPtr handle);

        [DllImport("XLUE.dll",CallingConvention=CallingConvention.Cdecl)]
        public static extern IntPtr lua_touserdata(IntPtr L, int idx);

        #endregion

        #region 注册对象和类辅助方法
        private static readonly int GLOBAL_API_SIZE = Marshal.SizeOf(typeof(XLLRTGlobalAPI));
        public static void RegisterGlobalObject(IntPtr hEnviroment, XLRTObjectInfo info)
        {
            int i = 0;
            var pClassMember = Marshal.AllocHGlobal(GLOBAL_API_SIZE * (info.Methods.Count+1));
            foreach (var methodInfo in info.Methods) {
                var pClassMemberGlobalAPI = new XLLRTGlobalAPI() {
                    permission = 0,
                    funName = methodInfo.Key,
                    func = methodInfo.Value
                };
                var pos = pClassMember.ToInt32() + i * GLOBAL_API_SIZE;
                var pClassMemberPos = new IntPtr(pos);
                Marshal.StructureToPtr(pClassMemberGlobalAPI, pClassMemberPos, false);
                i++;
            }
            var pNullMemberGlobalAPI = new XLLRTGlobalAPI() {
                permission = 0,
                funName = null,
                func = null
            };
            var pNullMemberPos = new IntPtr(pClassMember.ToInt32()+info.Methods.Count*GLOBAL_API_SIZE);
            Marshal.StructureToPtr(pNullMemberGlobalAPI, pNullMemberPos, false);

            var factoryObject = new XLLRTObject() {
                className = info.ClassName,
                objName = info.ObjectName,
                userData = info.UserData,
                pfnGetObject = info.GetFunction,
                memberFunctions = pClassMember
            };
            XLLuaRuntime.XLLRT_RegisterGlobalObj(hEnviroment, factoryObject);
            Marshal.FreeHGlobal(pClassMember);
        }
        public static void RegisterClass(IntPtr hEnviroment, XLRTClassInfo info)
        {
            int i = 0;
            var pClassMember = Marshal.AllocHGlobal(GLOBAL_API_SIZE * (info.Methods.Count+2));
            foreach (var methodInfo in info.Methods) {
                var pClassMemberGlobalAPI = new XLLRTGlobalAPI() {
                    permission = 0,
                    funName = methodInfo.Key,
                    func = methodInfo.Value
                };
                var pClassMemberPos = new IntPtr(pClassMember.ToInt32() + i * GLOBAL_API_SIZE);
                Marshal.StructureToPtr(pClassMemberGlobalAPI, pClassMemberPos, false);
                i++;
            }
            var pDeleteMemberGlobalAPI = new XLLRTGlobalAPI() {
                permission = 0,
                funName = "__gc",
                func = info.DeleteFunction
            };
            var pDeleteMemberPos = new IntPtr(pClassMember.ToInt32() + info.Methods.Count * GLOBAL_API_SIZE);
            Marshal.StructureToPtr(pDeleteMemberGlobalAPI, pDeleteMemberPos, false);

            var pNullMemberGlobalAPI = new XLLRTGlobalAPI() {
                permission = 0,
                funName = null,
                func = null
            };
            var pNullMemberPos = new IntPtr(pClassMember.ToInt32() + (info.Methods.Count+1) * GLOBAL_API_SIZE);
            Marshal.StructureToPtr(pNullMemberGlobalAPI, pNullMemberPos, false);

            XLLuaRuntime.XLLRT_RegisterClass(hEnviroment, info.ClassName, pClassMember, info.FatherClassName, 0);
            Marshal.FreeHGlobal(pClassMember);
        }
        public static void RegisterGlobalAPI(IntPtr hEnviroment, XLRTMethodsInfo info)
        {
            int i = 0;
            foreach (var methodInfo in info.Methods) {
                var pClassMemberGlobalAPI = new XLLRTGlobalAPI() {
                    permission = 0,
                    funName = methodInfo.Key,
                    func = methodInfo.Value
                };
                XLLRT_RegisterGlobalAPI(hEnviroment, pClassMemberGlobalAPI);
                Console.WriteLine("Return {0}", XLLRT_IsGlobalAPIRegistered(hEnviroment, methodInfo.Key));
                if (XLLRT_IsGlobalAPIRegistered(hEnviroment, methodInfo.Key)==IntPtr.Zero) {
                    Console.WriteLine("Func {0} has been registed!",methodInfo.Key);
                }
                i++;
            }
        }
        #endregion

        #region 全局方法迭代器
        public static void PrintAllGlobalMethods()
        {
            IntPtr hEnviroment = XLLuaRuntime.XLLRT_GetEnv(new IntPtr(0));
            IntPtr hEnum = XLLRT_BeginEnumGlobalAPI(hEnviroment);
            XLLRTGlobalAPI luaAPI = new XLLRTGlobalAPI();
            IntPtr luaAPIPtr = Marshal.AllocHGlobal(GLOBAL_API_SIZE);
            while(XLLRT_GetNextGlobalAPI(hEnum, luaAPIPtr))
            {
                luaAPI = (XLLRTGlobalAPI)Marshal.PtrToStructure(luaAPIPtr, typeof(XLLRTGlobalAPI));
	            Console.WriteLine("name={0}", luaAPI.funName);
            }
            XLLRT_EndEnum(hEnum);
        }
        public static void PrintAllGlobalClasses()
        {
            IntPtr hEnviroment = XLLuaRuntime.XLLRT_GetEnv(new IntPtr(0));
            IntPtr hEnum = XLLRT_BeginEnumGlobalClass(hEnviroment);
            XLLRTClass luaClass = new XLLRTClass();
            IntPtr luaClassPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(XLLRTClass)));
            while (XLLRT_GetNextGlobalClass(hEnum, luaClassPtr)) {
                luaClass = (XLLRTClass)Marshal.PtrToStructure(luaClassPtr, typeof(XLLRTClass));
                if (luaClass.className.Contains("Tree")) {
                    Console.WriteLine("name={0}", luaClass.className);
                    IntPtr luaAPIPtr = luaClass.MemberFunctions;
                    IntPtr pClassMemberPos = luaAPIPtr;
                    XLLRTGlobalAPI luaAPI = (XLLRTGlobalAPI)Marshal.PtrToStructure(pClassMemberPos, typeof(XLLRTGlobalAPI));
                    int i = 1;
                    while (luaAPI.funName != null) {
                        Console.WriteLine("name={0}", luaAPI.funName);
                        var pos = luaAPIPtr.ToInt32() + i * GLOBAL_API_SIZE;
                        pClassMemberPos = new IntPtr(pos);
                        luaAPI = (XLLRTGlobalAPI)Marshal.PtrToStructure(pClassMemberPos, typeof(XLLRTGlobalAPI));
                        i++;
                    }
                    break;
                }
            }
            XLLRT_EndEnum(hEnum);
        }
        #endregion
    }
    
}
