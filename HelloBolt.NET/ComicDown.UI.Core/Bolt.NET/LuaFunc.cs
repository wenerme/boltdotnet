using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComicDown.UI.Core.Bolt
{
    public sealed class LuaFunc<T> : LuaBaseFunctor
    {
        private LuaFunc(IntPtr luaState) : base(luaState) { }

        private Func<T> Create(Func<T> caller)
        {
            return () => {
                BeginCall();
                T result = caller();
                EndCall();
                return result;
            };
        }

        public static Func<T> Create(IntPtr luaState, Func<T> caller)
        {
            var instance = new LuaFunc<T>(luaState);
            return instance.Create(caller);
        }
    }
    public sealed class LuaFunc<T1,T2> : LuaBaseFunctor
    {
        private LuaFunc(IntPtr luaState) : base(luaState) { }

        private Func<T1,T2> Create(Func<T1,T2> caller)
        {
            return (t1) => {
                BeginCall();
                T2 t2 = caller(t1);
                EndCall();
                return t2;
            };
        }

        public static Func<T1, T2> Create(IntPtr luaState, Func<T1, T2> caller)
        {
            var instance = new LuaFunc<T1,T2>(luaState);
            return instance.Create(caller);
        }
    }
    public sealed class LuaFunc<T1, T2,T3> : LuaBaseFunctor
    {
        private LuaFunc(IntPtr luaState) : base(luaState) { }

        private Func<T1, T2, T3> Create(Func<T1, T2, T3> caller)
        {
            return (t1, t2) => {
                BeginCall();
                T3 t3=caller(t1, t2);
                EndCall();
                return t3;
            };
        }

        public static Func<T1, T2,T3> Create(IntPtr luaState, Func<T1, T2,T3> caller)
        {
            var instance = new LuaFunc<T1, T2,T3>(luaState);
            return instance.Create(caller);
        }
    }
    public sealed class LuaFunc<T1, T2, T3,T4> : LuaBaseFunctor
    {
        private LuaFunc(IntPtr luaState) : base(luaState) { }

        private Func<T1, T2, T3,T4> Create(Func<T1, T2, T3,T4> caller)
        {
            return (t1, t2, t3) => {
                BeginCall();
                T4 t4 = caller(t1, t2, t3);
                EndCall();
                return t4;
            };
        }

        public static Func<T1, T2, T3,T4> Create(IntPtr luaState, Func<T1, T2, T3,T4> caller)
        {
            var instance = new LuaFunc<T1, T2, T3,T4>(luaState);
            return instance.Create(caller);
        }
    }
    public sealed class LuaFunc<T1, T2, T3, T4,T5> : LuaBaseFunctor
    {
        private LuaFunc(IntPtr luaState) : base(luaState) { }

        private Func<T1, T2, T3, T4,T5> Create(Func<T1, T2, T3, T4,T5> caller)
        {
            return (t1, t2, t3, t4) => {
                BeginCall();
                T5 t5 = caller(t1, t2, t3, t4);
                EndCall();
                return t5;
            };
        }

        public static Func<T1, T2, T3, T4, T5> Create(IntPtr luaState, Func<T1, T2, T3, T4,T5> caller)
        {
            var instance = new LuaFunc<T1, T2, T3, T4,T5>(luaState);
            return instance.Create(caller);
        }
    }
}
