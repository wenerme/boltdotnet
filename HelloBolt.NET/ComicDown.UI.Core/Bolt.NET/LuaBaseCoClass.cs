using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;

namespace ComicDown.UI.Core.Bolt
{
    public abstract class LuaBaseCoClass<T,LT> : LuaBase
    {
        #region 私有静态成员变量
        //
        //类型信息
        //
        private static string _typeFullName;
        private static Dictionary<string, LuaCFunction> _lua_functions = new Dictionary<string, LuaCFunction>();
        private static LuaCFunction _lua_DeleteInstance = DeleteInstance;
        #endregion

        #region 构造函数
        static LuaBaseCoClass()
        {
            CollectTypeInformations();
            CollectLuaClassMembers();
        }
        #endregion

        #region 私有静态方法
        private static IntPtr GetClassFactoryInstance(IntPtr ud)
        {
            IntPtr pNULL = new IntPtr(0);
            return pNULL;
        }
        private static int DeleteInstance(IntPtr luaState)
        {
            IntPtr instancePointer = XLLuaRuntime.luaL_checkudata(luaState, 1, _typeFullName);
            IntPtr instanceIndex = new IntPtr(Marshal.ReadInt32(instancePointer));
            Console.WriteLine("Delte CoClass:{0}",_typeFullName);
            return 0;
        }

        private static void CollectTypeInformations()
        {
            var ttype = typeof(T);
            _typeFullName = ttype.FullName;
        }
        private static void CollectLuaClassMembers()
        {
            //收集需要注册到Lua环境的类成员函数信息
            var ttype = typeof(LT);
            foreach (MethodInfo method in ttype.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)) {
                foreach (LuaClassMethodAttribute attribute in method.GetCustomAttributes(typeof(LuaClassMethodAttribute), true)) {
                    var name = attribute.HasName ? attribute.Name : method.Name;
                    var permission = attribute.Permission;
                    var deleteOld = attribute.DeleteOld;
                    var classMemberFunction = (LuaCFunction)Delegate.CreateDelegate(typeof(LuaCFunction), method);
                    if (!_lua_functions.ContainsKey(name)) {
                        _lua_functions.Add(name, classMemberFunction);
                    }
                }
            }
        }
        #endregion

        #region 公有静态方法
        public static void Register()
        {
            //
            //0、得到Lua环境句柄
            //
            IntPtr pNULL = new IntPtr(0);
            IntPtr hEnviroment = XLLuaRuntime.XLLRT_GetEnv(pNULL);

            //2、注册类型
            var classInfo = new XLRTClassInfo() {
                ClassName = _typeFullName,
                FatherClassName = null,
                DeleteFunction = _lua_DeleteInstance,
                Methods = _lua_functions
            };
            XLLuaRuntime.RegisterClass(hEnviroment, classInfo);
        }
        public static int GetIndex(IntPtr luaState)
        {
            IntPtr instancePointer = XLLuaRuntime.luaL_checkudata(luaState, 1, _typeFullName);
            IntPtr instanceIndex = new IntPtr(Marshal.ReadInt32(instancePointer));
            return instanceIndex.ToInt32();
        }
        #endregion
    }
}
