using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections;
using System.Windows.Forms;

namespace ComicDown.UI.Core.Bolt
{
    internal sealed class Hook { }

    /// <summary>
    /// Xunlei Bolt .NET Wrapper
    /// </summary>
    public sealed class XLBolt
    {
        #region Singleton
        
        /// <summary>
        /// XLBolt静态实例
        /// </summary>
        private static XLBolt instance = null;

        /// <summary>
        /// invoke action queue
        /// </summary>
        private Queue<System.Action> invokeActions;

        /// <summary>
        /// invoke message
        /// </summary>
        public static uint invokeActionMessage = (uint)(WM_USER)+10;

        /// <summary>
        /// invoke message name
        /// </summary>
        private static readonly string XLBOLT_INVOKE_ACTION = "XLBOLT_INVOKE_ACTION";

        static int count = 0;

        private static object locker = new object();
        private static object locker2 = new object();
        private static bool isMessageLoopBegin = false;
        private static fnLuaErrorHandle errorHandle = XLBolt.BoltErrorHandle;
        private static BackGroundForm backGroundForm = new BackGroundForm();

        private static bool IsMessageLoopBegin
        {
            get {
                lock (locker) {
                    return XLBolt.isMessageLoopBegin;  
                }
            }
            set {
                lock (locker) {
                    XLBolt.isMessageLoopBegin = value;  
                }
            }
        }

        /// <summary>
        /// 主线程ID
        /// </summary>
        private uint threadID;

        /// <summary>
        /// 静态构造函数，单例
        /// </summary>
        /// <returns></returns>
        public static XLBolt Instance()
        {
            if (instance == null) {
                instance = new XLBolt();
            }
            return instance;
        }

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private XLBolt()
        {
            backGroundForm.Visible = false;
            invokeActions = new Queue<Action>();
            backGroundForm.TimerTick += new Action(backGroundForm_TimerTick);
        }

        void backGroundForm_TimerTick()
        {
            InvokeActions();
        }
        
        #endregion


        #region Public Methods
        /// <summary>
        /// 运行Bolt
        /// </summary>
        /// <param name="xarSearchPath"></param>
        /// <param name="xar"></param>
        /// <param name="callback"></param>
        public void Run(string xarSearchPath, string xar, Action callback)
        {
            Initialization("");
            XLLuaRuntime.XLLRT_ErrorHandle(errorHandle);
            AddXARSearchPath(xarSearchPath);
            callback();
            LoadXAR(xar);
            MessageLoop();
        }
        public static int BoltErrorHandle(IntPtr luaState, string pExtInfo, string luaErrorString, IntPtr pStackInfo)
        {
#if DEBUG_BOLT
            StringBuilder stackBuffer = new StringBuilder(1024);
            XLUE_GetLuaStack(luaState, stackBuffer, 1024);
            ConsoleHelper.WriteLineWithColor(ConsoleColor.Red, "\n──────────────────────────────────");
            ConsoleHelper.WriteLineWithColor(ConsoleColor.Red, "★BOLT错误：");
            ConsoleHelper.WriteLineWithColor(ConsoleColor.Red, "──────────────────────────────────");
            ConsoleHelper.WriteLineWithColor(ConsoleColor.Red, pExtInfo);
            ConsoleHelper.WriteLineWithColor(ConsoleColor.Red, luaErrorString);
            ConsoleHelper.WriteLineWithColor(ConsoleColor.Red, "─────────\n");
            ConsoleHelper.WriteLineWithColor(ConsoleColor.Red, stackBuffer.ToString());
            ConsoleHelper.WriteLineWithColor(ConsoleColor.Red, "─────────\n");
#endif
            return 0;
        }

        /// <summary>
        /// 将方法委托给主线程执行
        /// </summary>
        /// <param name="action"></param>
        public void Invoke(System.Action action)
        {
            //如果委托队列不存在，则创建
            Monitor.Enter(this);
            try {
                if (invokeActions == null) {
                    invokeActions = new Queue<System.Action>();
                }
            } finally {
                Monitor.Exit(this);
            }


            //将action添加到队列
            Monitor.Enter(invokeActions);
            try {
                if (XLBolt.invokeActionMessage == 0) {
                    XLBolt.invokeActionMessage = RegisterWindowMessage(XLBOLT_INVOKE_ACTION);
                }
                invokeActions.Enqueue(action);
            } finally {
                Monitor.Exit(invokeActions);
            }

            //异步推送委托执行Action消息
            if (IsMessageLoopBegin) {
                var success = PostThreadMessage(this.threadID, XLBolt.invokeActionMessage, UIntPtr.Zero, IntPtr.Zero);
                if (!success) {
                    
                }
            }
        }
        #endregion

        #region Wrapper API
        /// <summary>
        /// Initialize XLBolt 
        /// </summary>
        /// <param name="boltPath"></param>
        /// <returns></returns>
        private bool Initialization(String boltPath)
        {
            XL_InitGraphicLib(0);
            XL_SetFreeTypeEnabled(1);
            XLUE_InitLoader(0);
            return true;
        }

        /// <summary>
        /// Add xar search path
        /// </summary>
        /// <param name="newFolderPath"></param>
        private void AddXARSearchPath(String newFolderPath)
        {
            XLUE_AddXARSearchPath(newFolderPath);
        }

        /// <summary>
        /// Load xar directory or package
        /// </summary>
        /// <param name="xarName">xar directory name or package name</param>
        private void LoadXAR(String xarName)
        {
            XLUE_LoadXAR(xarName);
        }

        /// <summary>
        /// 消息循环
        /// </summary>
        private void MessageLoop()
        {
            this.threadID = (uint)GetCurrentThreadId();
            MSG msg = new MSG();
            PeekMessage(ref msg, IntPtr.Zero, WM_USER, WM_USER, PM_NOREMOVE);
            IsMessageLoopBegin = true;
            while (GetMessage(ref msg, IntPtr.Zero, 0, 0)) {
                if (msg.message == WM_QUIT)
                    return;
                if (ThreadMessageProc(ref msg)) {
                    continue;
                }
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        /// <summary>
        /// 线程消息循环
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool ThreadMessageProc(ref MSG msg)
        {
            if (msg.message == invokeActionMessage) {
                InvokeActions();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 执行其他线程委托的Actino队列
        /// </summary>
        private void InvokeActions()
        {
            if (invokeActions == null) return;
            if (Monitor.TryEnter(invokeActions)) {
                try {
                    if (invokeActions.Count == 0) {
                        return;
                    }

                    Action action = null;
                    if (invokeActions.Count > 0) { 
                        action = invokeActions.Dequeue();
                    }
                    while (action != null) {
                        Monitor.Exit(invokeActions);
                        action();
                        Monitor.Enter(invokeActions);
                        if (invokeActions.Count > 0) {
                            action = invokeActions.Dequeue();
                        } else {
                            action = null;
                        }
                    }
                } catch (Exception e) {

                } finally {
                    Monitor.Exit(invokeActions);
                }
            }
        }
        #endregion

        #region Add Dll Path
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);
        static XLBolt()
        {
            //SetDllDirectory(AppDomain.CurrentDomain.BaseDirectory + "libbolt");
        }
        #endregion

        #region XLUE API
        [DllImport("XLUE.dll", EntryPoint = "XL_InitGraphicLib")]
        public static extern long XL_InitGraphicLib(int theParam);

        [DllImport("XLUE.dll", EntryPoint = "XL_SetFreeTypeEnabled")]
        public static extern long XL_SetFreeTypeEnabled(int isEnable);

        [DllImport("XLUE.dll", EntryPoint = "XLUE_InitLoader")]
        public static extern long XLUE_InitLoader(int theParam);

        [DllImport("XLUE.dll", CharSet = CharSet.Unicode, EntryPoint = "XLUE_AddXARSearchPath")]
        public static extern long XLUE_AddXARSearchPath(String theParam);

        [DllImport("XLUE.dll", CharSet = CharSet.Ansi, EntryPoint = "XLUE_LoadXAR")]
        public static extern long XLUE_LoadXAR(String xarName);

        [DllImport("XLUE.dll",CallingConvention=CallingConvention.StdCall)]
        public static extern long XLUE_GetLuaStack(IntPtr luaState,StringBuilder lpStackBuffer, int bufferSize);
        #endregion
         
        #region WIN32
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool TranslateMessage(ref MSG msg);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int DispatchMessage(ref MSG msg);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool GetMessage(ref MSG msg, IntPtr hwnd, int minFilter, int maxFilter);

        [DllImport("user32.dll",SetLastError=true, CharSet = CharSet.Auto)]
        static extern uint RegisterWindowMessage(string msg);

        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(
            IntPtr hWnd,        // 信息发往的窗口的句柄
            uint Msg,           // 消息ID
            IntPtr wParam,      // 参数1
            IntPtr lParam       // 参数2
        );

        [DllImport("user32.dll",SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostThreadMessage(uint id, uint msg, UIntPtr wparam, IntPtr lparam);

        [DllImport("user32.dll")]
        static extern bool PeekMessage([MarshalAs(UnmanagedType.Struct)] ref MSG lpMsg, IntPtr hwnd, int wMsgFilterMin, int wMsgFilterMax, int wRemoveMsg);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll")]
        static extern bool SetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);


        [DllImport("user32.dll")]
        static extern bool WaitMessage();

        [DllImport("kernel32")]
        public static extern int GetCurrentThreadId();

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public int message;
            public IntPtr wParam;
            public IntPtr lParam;
            public int time;
            public int pt_x;
            public int pt_y;
        }

        const uint INFINITE = 0xFFFFFFFF;
        const uint WAIT_ABANDONED = 0x00000080;
        const uint WAIT_OBJECT_0 = 0x00000000;
        const uint WAIT_TIMEOUT = 0x00000102;
        const int PM_NOREMOVE = 0x0000;
        const int PM_REMOVE   = 0x0001;
        const int PM_NOYIELD = 0x0002;
        const int WM_QUIT = 0x0012;
        const int WM_USER = 0x0400;
        const int WM_TIMER = 0x0113;
        #endregion
    }
}
