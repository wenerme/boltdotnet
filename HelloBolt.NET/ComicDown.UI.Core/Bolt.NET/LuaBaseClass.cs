using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;

namespace ComicDown.UI.Core.Bolt
{
    public abstract class LuaBaseClass<T, LT> : LuaBase
        where T:new()
    {
        #region 私有静态成员变量
        //
        //类型池
        //
        private static Dictionary<int, T> _instancesDict;
        private static int _currentIndex;
        private static T _instance;

        //
        //类型工厂信息
        //
        private static XLLRTFuncGetObject _lua_GetClassFactoryInstance = GetClassFactoryInstance;
        private static LuaCFunction _lua_CreateInstance = CreateInstance;
        private static LuaCFunction _lua_DeleteInstance = DeleteInstance;

        //
        //类型信息
        //
        private static CreatePolicy _createPolicy;
        private static string _typeFullName;
        private static string _typeFactoryClassName;
        private static string _typeFactoryObjectName;
        private static Dictionary<string, LuaCFunction> _lua_functions = new Dictionary<string, LuaCFunction>();
        #endregion

        #region 构造函数
        static LuaBaseClass()
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
        private static int CreateInstance(IntPtr luaState)
        {
            T instance = new T();
            _instancesDict[_currentIndex] = instance;
            XLLuaRuntime.XLLRT_PushXLObject(luaState, _typeFullName, new IntPtr(_currentIndex));
            _currentIndex++;
            return 1;
        }
        private static int DeleteInstance(IntPtr luaState)
        {
            IntPtr instancePointer = XLLuaRuntime.luaL_checkudata(luaState, 1, _typeFullName);
            IntPtr instanceIndex = new IntPtr(Marshal.ReadInt32(instancePointer));
            _instancesDict[instanceIndex.ToInt32()] = default(T);
            _instancesDict.Remove(instanceIndex.ToInt32());
            return 0;
        }

        private static void CollectTypeInformations()
        {
            var ttype = typeof(T);
            _typeFullName = ttype.FullName;
            _typeFactoryClassName = _typeFullName + ".Factory.Class";
            _typeFactoryObjectName = _typeFullName + ".Factory";
        }
        private static void CollectLuaClassMembers()
        {
            //收集需要注册到Lua环境的类成员函数信息
            var ttype = typeof(LT);
            foreach (MethodInfo method in ttype.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)) {
                foreach (LuaClassMethodAttribute attribute in method.GetCustomAttributes(typeof(LuaClassMethodAttribute), true)) {
                    var name = attribute.HasName ? attribute.Name : method.Name;
                    var classMemberFunction = (LuaCFunction)Delegate.CreateDelegate(typeof(LuaCFunction), method);
                    if (!_lua_functions.ContainsKey(name)) {
                        _lua_functions.Add(name, classMemberFunction);
                    }
                }
            }

            //搜集类型创建策略，如果找不到就默认为New
            bool foundPolicy = false;
            foreach (LuaClassAttribute attribute in ttype.GetCustomAttributes(typeof(LuaClassAttribute),true)) {
                _createPolicy = attribute.CreatePolicy;
                foundPolicy = true;
                if (_createPolicy == CreatePolicy.Singleton) {
                    _instance = new T();
                }
                if (_createPolicy == CreatePolicy.Factory) {
                    _instancesDict = new Dictionary<int, T>();
                }
                break;
            }
            if (!foundPolicy) {
                _createPolicy = CreatePolicy.Factory;
                _instancesDict = new Dictionary<int, T>();
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


            if (_createPolicy == CreatePolicy.Factory) {
                //1、注册类型工厂
                var factoryObjectInfo = new XLRTObjectInfo() {
                    ClassName = _typeFactoryClassName,
                    ObjectName = _typeFactoryObjectName,
                    UserData = pNULL,
                    GetFunction = _lua_GetClassFactoryInstance,
                    Methods = new Dictionary<string, LuaCFunction>{
                        {"CreateInstance",_lua_CreateInstance}
                    }
                };
                XLLuaRuntime.RegisterGlobalObject(hEnviroment, factoryObjectInfo);

                //2、注册类型
                var classInfo = new XLRTClassInfo() {
                    ClassName = _typeFullName,
                    FatherClassName = null,
                    DeleteFunction = _lua_DeleteInstance,
                    Methods = _lua_functions
                };
                XLLuaRuntime.RegisterClass(hEnviroment, classInfo);
            } else if (_createPolicy == CreatePolicy.Singleton) {
                //1、注册全局对象
                var singletonObjectInfo = new XLRTObjectInfo() {
                    ClassName = _typeFullName,
                    ObjectName = _typeFullName,
                    UserData = pNULL,
                    GetFunction = _lua_GetClassFactoryInstance,
                    Methods = _lua_functions
                };
                XLLuaRuntime.RegisterGlobalObject(hEnviroment, singletonObjectInfo);
            } else {
                throw new Exception("UnSupport CreatePolicy!");
            }
        }
        public static T GetInstance(IntPtr luaState)
        {
            if (_createPolicy == CreatePolicy.Factory) {
                IntPtr instancePointer = XLLuaRuntime.luaL_checkudata(luaState, 1, _typeFullName);
                IntPtr instanceIndex = new IntPtr(Marshal.ReadInt32(instancePointer));
                T instance = _instancesDict[instanceIndex.ToInt32()];
                return instance;
            }
            if (_createPolicy == CreatePolicy.Singleton) {
                return _instance;
            }
            return default(T);
        }

        #endregion
    }
}