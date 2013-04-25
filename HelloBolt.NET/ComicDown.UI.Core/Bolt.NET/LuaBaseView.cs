using System;
using System.Collections.Generic;
using System.Text;
using ComicDown.UI.Core.Bolt;

namespace ComicDown.UI.Core.Bolt
{
    public abstract class LuaBaseView
    {
        private int enviromentID = 0;
        public LuaBaseView() { }

        protected IntPtr GetLuaRuntime()
        {
            IntPtr pNULL = new IntPtr(0);
            IntPtr hEnviroment = XLLuaRuntime.XLLRT_GetEnv(pNULL);
            IntPtr hRuntime = XLLuaRuntime.XLLRT_GetRuntime(hEnviroment, null);
            return hRuntime;
        }

        protected IntPtr GetLuaState()
        {
            IntPtr pNULL = new IntPtr(0);
            IntPtr hEnviroment = XLLuaRuntime.XLLRT_GetEnv(pNULL);
            IntPtr hRuntime = XLLuaRuntime.XLLRT_GetRuntime(hEnviroment, null);
            IntPtr L = XLLuaRuntime.XLLRT_GetLuaState(hRuntime);
            return L;
        }

        protected IntPtr GetLuaChunk(string file,string func)
        {
            var luaChunck = "".CreateChunkFromModule(file, func);
            return luaChunck;
        }
    }
}
