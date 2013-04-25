using System;
using System.Collections.Generic;
using System.Text;

namespace ComicDown.UI.Core.Bolt
{
    public abstract class LuaBaseFunctor:LuaBase
    {
        protected IntPtr _luaState;
        protected int _luaFunctionRefIndex;
        protected int _lastTopIndex;
        public LuaBaseFunctor(IntPtr luaState)
        {
            _luaState = luaState;
            _luaFunctionRefIndex = luaState.GetFuncRef();
        }
        protected override void OnDisposeUnmangedResources()
        {
            base.OnDisposeUnmangedResources();
            XLBolt.Instance().Invoke(() => {
                if (_luaState != IntPtr.Zero) {
                    XLLuaRuntime.luaL_unref(
                        _luaState,
                        (int)LuaInnerIndex.LUA_REGISTRYINDEX,
                        _luaFunctionRefIndex);
                    _luaState = IntPtr.Zero;
                    _luaFunctionRefIndex = 0;
                }
            });
        }
        protected void BeginCall()
        {
            _lastTopIndex = XLLuaRuntime.lua_gettop(_luaState);
            XLLuaRuntime.lua_rawgeti(_luaState, (int)LuaInnerIndex.LUA_REGISTRYINDEX, _luaFunctionRefIndex);
        }
        protected void EndCall()
        {
            XLLuaRuntime.lua_settop(_luaState, _lastTopIndex);   
        }
    }
}
