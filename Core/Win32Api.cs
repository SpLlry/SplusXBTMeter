using System.Runtime.InteropServices;
using System.Runtime.Versioning;
namespace SplusXBTMeter.Core
{
    [SupportedOSPlatform("windows")]
    public static partial class Win32Api
    {
        #region API 声明

        #region 基础窗口 API
        /// <summary>
        /// 查找顶级窗口
        /// </summary>
        /// <param name="lpClassName">窗口类名</param>
        /// <param name="lpWindowName">窗口标题</param>
        /// <returns>窗口句柄</returns>
        [LibraryImport("user32.dll", EntryPoint = "FindWindowW", StringMarshalling = StringMarshalling.Utf16)]
        public static partial IntPtr FindWindowW(string lpClassName, string lpWindowName);

        /// <summary>
        /// 查找子窗口
        /// </summary>
        /// <param name="hWndParent">父窗口句柄</param>
        /// <param name="hWndChildAfter">子窗口句柄</param>
        /// <param name="lpClassName">窗口类名</param>
        /// <param name="lpWindowName">窗口标题</param>
        /// <returns>窗口句柄</returns>
        [LibraryImport("user32.dll", EntryPoint = "FindWindowExW", StringMarshalling = StringMarshalling.Utf16)]
        public static partial IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpClassName, string lpWindowName);

        /// <summary>
        /// 设置父窗口
        /// </summary>
        /// <param name="hWndChild">子窗口句柄</param>
        /// <param name="hWndNewParent">新父窗口句柄</param>
        /// <returns>原父窗口句柄</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        /// <summary>
        /// 获取窗口矩形
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="lpRect">窗口矩形</param>
        /// <returns>是否成功</returns>
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        /// <summary>
        /// 设置窗口位置和大小
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="hWndInsertAfter">Z序位置</param>
        /// <param name="X">新X坐标</param>
        /// <param name="Y">新Y坐标</param>
        /// <param name="cx">新宽度</param>
        /// <param name="cy">新高度</param>
        /// <param name="uFlags">标志位</param>
        /// <returns>是否成功</returns>
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        /// <summary>
        /// 设置窗口长整型属性（64位兼容）
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="nIndex">属性索引</param>
        /// <param name="dwNewLong">新值</param>
        /// <returns>原值</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        /// <summary>
        /// 获取窗口长整型属性（64位兼容）
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="nIndex">属性索引</param>
        /// <returns>当前值</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        /// <summary>
        /// 调用原始窗口过程
        /// </summary>
        /// <param name="lpPrevWndFunc">原窗口过程</param>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="Msg">消息ID</param>
        /// <param name="wParam">附加参数</param>
        /// <param name="lParam">附加参数</param>
        /// <returns>处理结果</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 设置Windows事件钩子
        /// </summary>
        /// <param name="eventMin">最小事件ID</param>
        /// <param name="eventMax">最大事件ID</param>
        /// <param name="hmodWinEventProc">模块句柄</param>
        /// <param name="lpfnWinEventProc">回调函数</param>
        /// <param name="idProcess">进程ID</param>
        /// <param name="idThread">线程ID</param>
        /// <param name="dwFlags">标志位</param>
        /// <returns>钩子句柄</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(
          uint eventMin,
          uint eventMax,
          IntPtr hmodWinEventProc,
          WinEventDelegate lpfnWinEventProc,
          uint idProcess,
          uint idThread,
          uint dwFlags);

        /// <summary>
        /// 移除Windows事件钩子
        /// </summary>
        /// <param name="hWinEventHook">钩子句柄</param>
        /// <returns>是否成功</returns>
        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);
        #endregion

        #region 注册表 API
        /// <summary>
        /// 打开注册表项
        /// </summary>
        /// <param name="hKey">根键句柄</param>
        /// <param name="lpSubKey">子键路径</param>
        /// <param name="ulOptions">保留选项</param>
        /// <param name="samDesired">访问权限</param>
        /// <param name="phkResult">返回键句柄</param>
        /// <returns>错误码</returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegOpenKeyEx(
            IntPtr hKey,
            string lpSubKey,
            int ulOptions,
            int samDesired,
            out IntPtr phkResult);

        /// <summary>
        /// 监视注册表项变化
        /// </summary>
        /// <param name="hKey">键句柄</param>
        /// <param name="bWatchSubtree">是否监视子树</param>
        /// <param name="dwNotifyFilter">通知过滤器</param>
        /// <param name="hEvent">事件句柄</param>
        /// <param name="fAsynchronous">是否异步</param>
        /// <returns>错误码</returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegNotifyChangeKeyValue(
            IntPtr hKey,
            bool bWatchSubtree,
            int dwNotifyFilter,
            IntPtr hEvent,
            bool fAsynchronous);

        /// <summary>
        /// 查询注册表值
        /// </summary>
        /// <param name="hKey">键句柄</param>
        /// <param name="lpValueName">值名称</param>
        /// <param name="lpReserved">保留参数</param>
        /// <param name="lpType">值类型</param>
        /// <param name="lpData">数据缓冲区</param>
        /// <param name="lpcbData">数据长度</param>
        /// <returns>错误码</returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegQueryValueEx(
            IntPtr hKey,
            string lpValueName,
            int lpReserved,
            out int lpType,
            byte[] lpData,
            ref int lpcbData);

        /// <summary>
        /// 关闭注册表项
        /// </summary>
        /// <param name="hKey">键句柄</param>
        /// <returns>错误码</returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegCloseKey(IntPtr hKey);

        /// <summary>
        /// 打开注册表项（Unicode版本）
        /// </summary>
        /// <param name="hKey">根键</param>
        /// <param name="lpSubKey">子键路径</param>
        /// <param name="ulOptions">选项</param>
        /// <param name="samDesired">访问权限</param>
        /// <param name="phkResult">返回句柄</param>
        /// <returns>错误码</returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int RegOpenKeyExW(
            uint hKey,
            string lpSubKey,
            uint ulOptions,
            uint samDesired,
            out IntPtr phkResult);

        /// <summary>
        /// 查询注册表值（Unicode版本）
        /// </summary>
        /// <param name="hKey">键句柄</param>
        /// <param name="lpValueName">值名称</param>
        /// <param name="lpReserved">保留</param>
        /// <param name="lpType">值类型</param>
        /// <param name="lpData">数据缓冲区</param>
        /// <param name="lpcbData">数据长度</param>
        /// <returns>错误码</returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int RegQueryValueExW(
            IntPtr hKey,
            string lpValueName,
            int lpReserved,
            out int lpType,
            byte[] lpData,
            ref int lpcbData);
        #endregion

        #region 托盘专用 API
        /// <summary>
        /// 操作系统托盘图标
        /// </summary>
        /// <param name="dwMessage">操作类型</param>
        /// <param name="lpData">托盘图标数据结构</param>
        /// <returns>是否成功</returns>
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA lpData);
        /// <summary>
        /// 销毁图标
        /// </summary>
        /// <param name="hIcon">图标句柄</param>
        /// <returns>是否成功</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);
        /// <summary>
        /// 获取鼠标光标位置
        /// </summary>
        /// <param name="pt">返回点坐标</param>
        /// <returns>是否成功</returns>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT pt);

        /// <summary>
        /// 创建弹出菜单
        /// </summary>
        /// <returns>菜单句柄</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr CreatePopupMenu();

        /// <summary>
        /// 追加菜单项
        /// </summary>
        /// <param name="hMenu">菜单句柄</param>
        /// <param name="uFlags">标志位</param>
        /// <param name="uIDNewItem">菜单项ID</param>
        /// <param name="lpNewItem">菜单项文本</param>
        /// <returns>是否成功</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

        /// <summary>
        /// 显示弹出菜单
        /// </summary>
        /// <param name="hMenu">菜单句柄</param>
        /// <param name="uFlags">标志位</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="nReserved">保留</param>
        /// <param name="hWnd">所属窗口</param>
        /// <param name="prcRect">矩形区域</param>
        /// <returns>选中菜单项ID</returns>
        [DllImport("user32.dll")]
        public static extern int TrackPopupMenu(IntPtr hMenu, int uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

        /// <summary>
        /// 将窗口置于前台
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns>是否成功</returns>
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// 销毁菜单
        /// </summary>
        /// <param name="hMenu">菜单句柄</param>
        /// <returns>是否成功</returns>
        [DllImport("user32.dll")]
        public static extern bool DestroyMenu(IntPtr hMenu);

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <param name="lpMsg">消息结构</param>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="wMsgFilterMin">最小消息过滤</param>
        /// <param name="wMsgFilterMax">最大消息过滤</param>
        /// <returns>是否获取到消息</returns>
        [DllImport("user32.dll")]
        public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        /// <summary>
        /// 转换消息
        /// </summary>
        /// <param name="lpMsg">消息结构</param>
        /// <returns>是否成功</returns>
        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(ref MSG lpMsg);

        /// <summary>
        /// 分发消息
        /// </summary>
        /// <param name="lpMsg">消息结构</param>
        /// <returns>处理结果</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage(ref MSG lpMsg);

        /// <summary>
        /// 加载图标
        /// </summary>
        /// <param name="hInstance">实例句柄</param>
        /// <param name="lpIconName">图标名称</param>
        /// <returns>图标句柄</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        /// <summary>
        /// 创建窗口
        /// </summary>
        /// <param name="dwExStyle">扩展样式</param>
        /// <param name="lpClassName">类名</param>
        /// <param name="lpWindowName">窗口名</param>
        /// <param name="dwStyle">样式</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="nWidth">宽度</param>
        /// <param name="nHeight">高度</param>
        /// <param name="hWndParent">父窗口</param>
        /// <param name="hMenu">菜单句柄</param>
        /// <param name="hInstance">实例句柄</param>
        /// <param name="lpParam">附加参数</param>
        /// <returns>窗口句柄</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        /// <summary>
        /// 加载图像（支持ICO文件）
        /// </summary>
        /// <param name="hInst">实例句柄</param>
        /// <param name="lpszName">图像路径</param>
        /// <param name="uType">图像类型</param>
        /// <param name="cxDesired">期望宽度</param>
        /// <param name="cyDesired">期望高度</param>
        /// <param name="fuLoad">加载标志</param>
        /// <returns>图像句柄</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr LoadImage(IntPtr hInst, string lpszName, int uType, int cxDesired, int cyDesired, int fuLoad);

        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="nCmdShow">显示命令</param>
        /// <returns>是否成功</returns>
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// 从资源创建图标
        /// </summary>
        /// <param name="presbits">资源数据</param>
        /// <param name="dwResSize">资源大小</param>
        /// <param name="fIcon">是否为图标</param>
        /// <param name="dwVer">版本号</param>
        /// <returns>图标句柄</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CreateIconFromResource(byte[] presbits, int dwResSize, bool fIcon, int dwVer);
        #endregion

        #endregion

        #region 结构体

        /// <summary>
        /// Windows事件回调委托
        /// </summary>
        /// <param name="hWinEventHook">钩子句柄</param>
        /// <param name="eventType">事件类型</param>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="idObject">对象ID</param>
        /// <param name="idChild">子对象ID</param>
        /// <param name="dwEventThread">事件线程ID</param>
        /// <param name="dwmsEventTime">事件时间</param>
        public delegate void WinEventDelegate(
            IntPtr hWinEventHook,
            uint eventType,
            IntPtr hwnd,
            int idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime);

        /// <summary>
        /// 矩形结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            /// <summary>左边界</summary>
            public int Left;
            /// <summary>上边界</summary>
            public int Top;
            /// <summary>右边界</summary>
            public int Right;
            /// <summary>下边界</summary>
            public int Bottom;
        }

        /// <summary>
        /// 系统托盘图标数据结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct NOTIFYICONDATA
        {
            /// <summary>结构大小</summary>
            public int cbSize;
            /// <summary>接收消息的窗口句柄</summary>
            public IntPtr hWnd;
            /// <summary>图标ID</summary>
            public int uID;
            /// <summary>标志位</summary>
            public int uFlags;
            /// <summary>回调消息ID</summary>
            public int uCallbackMessage;
            /// <summary>图标句柄</summary>
            public IntPtr hIcon;
            /// <summary>提示文本</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            /// <summary>状态</summary>
            public int dwState;
            /// <summary>状态掩码</summary>
            public int dwStateMask;
            /// <summary>气球提示文本</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            /// <summary>超时时间</summary>
            public int uTimeout;
            /// <summary>气球提示标题</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            /// <summary>气球提示标志</summary>
            public int dwInfoFlags;
            /// <summary>GUID标识</summary>
            public Guid guidItem;
        }

        /// <summary>
        /// 点坐标结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>X坐标</summary>
            public int x;
            /// <summary>Y坐标</summary>
            public int y;
        }

        /// <summary>
        /// 消息结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            /// <summary>窗口句柄</summary>
            public IntPtr hwnd;
            /// <summary>消息ID</summary>
            public uint message;
            /// <summary>附加参数</summary>
            public IntPtr wParam;
            /// <summary>附加参数</summary>
            public IntPtr lParam;
            /// <summary>消息时间</summary>
            public uint time;
            /// <summary>鼠标位置</summary>
            public POINT pt;
        }
        #endregion

        #region 常量

        #region 注册表常量
        /// <summary>HKEY_CURRENT_USER 根键</summary>
        public const uint HKEY_CURRENT_USER = 0x80000001;

        /// <summary>注册表读权限</summary>
        public const uint KEY_READ = 0x20019;

        /// <summary>注册表通知权限</summary>
        public const int KEY_NOTIFY = 0x0010;

        /// <summary>注册表值类型：字符串</summary>
        public const int REG_SZ = 1;

        /// <summary>注册表值类型：DWORD</summary>
        public const int REG_DWORD = 4;

        /// <summary>注册表变更通知：最后设置值</summary>
        public const int REG_NOTIFY_CHANGE_LAST_SET = 0x00000004;
        #endregion

        #region 窗口常量
        /// <summary>任务栏窗口类名</summary>
        public const string TASKBAR_CLASS = "Shell_TrayWnd";

        /// <summary>窗口位置变化事件</summary>
        public const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;

        /// <summary>事件钩子：进程外上下文</summary>
        public const uint WINEVENT_OUTOFCONTEXT = 0x0000;

        /// <summary>事件钩子：跳过自身进程</summary>
        public const uint WINEVENT_SKIPOWNPROCESS = 0x0002;

        /// <summary>获取窗口样式索引</summary>
        public const int GWL_STYLE = -16;

        /// <summary>获取扩展窗口样式索引</summary>
        public const int GWL_EXSTYLE = -20;

        /// <summary>获取窗口过程索引</summary>
        public const int GWL_WNDPROC = -4;

        /// <summary>窗口样式：子窗口</summary>
        public const uint WS_CHILD = 0x40000000;

        /// <summary>窗口样式：可见</summary>
        public const uint WS_VISIBLE = 0x10000000;

        /// <summary>窗口样式：弹出窗口</summary>
        public const int WS_POPUP = unchecked((int)0x80000000);

        /// <summary>显示窗口：隐藏</summary>
        public const int SW_HIDE = 0;

        /// <summary>显示窗口：还原</summary>
        public const int SW_RESTORE = 9;

        /// <summary>显示窗口：显示</summary>
        public const int SW_SHOW = 5;

        /// <summary>扩展样式：透明</summary>
        public const uint WS_EX_TRANSPARENT = 0x00000020;

        /// <summary>扩展样式：分层</summary>
        public const uint WS_EX_LAYERED = 0x00080000;

        /// <summary>SetWindowPos标志：忽略Z序</summary>
        public const uint SWP_NOZORDER = 0x0004;

        /// <summary>SetWindowPos标志：不激活窗口</summary>
        public const uint SWP_NOACTIVATE = 0x0010;

        /// <summary>SetWindowPos标志：发送帧改变消息</summary>
        public const uint SWP_FRAMECHANGED = 0x0020;

        /// <summary>SetWindowPos标志：不改变位置</summary>
        public const uint SWP_NOMOVE = 0x0002;

        /// <summary>SetWindowPos标志：不改变大小</summary>
        public const uint SWP_NOSIZE = 0x0001;
        #endregion

        #region 消息常量
        /// <summary>窗口位置改变消息</summary>
        public const int WM_WINDOWPOSCHANGED = 0x0047;

        /// <summary>窗口销毁消息</summary>
        public const int WM_DESTROY = 0x0002;

        /// <summary>自定义托盘消息</summary>
        public const int WM_TRAY_MSG = 0x400;

        /// <summary>右键释放消息</summary>
        public const int WM_RBUTTONUP = 0x0205;
        #endregion

        #region 托盘常量
        /// <summary>添加托盘图标</summary>
        public const int NIM_ADD = 0x00000000;

        /// <summary>删除托盘图标</summary>
        public const int NIM_DELETE = 0x00000002;

        /// <summary>托盘图标标志：图标</summary>
        public const int NIF_ICON = 0x00000002;

        /// <summary>托盘图标标志：提示</summary>
        public const int NIF_TIP = 0x00000004;

        /// <summary>托盘图标标志：消息</summary>
        public const int NIF_MESSAGE = 0x00000001;

        /// <summary>TrackPopupMenu标志：返回命令ID</summary>
        public const int TPM_RETURNCMD = 0x00000100;

        /// <summary>默认应用程序图标</summary>
        public static readonly IntPtr IDI_APPLICATION = new(32512);
        #endregion

        #region 图像常量
        /// <summary>图像类型：图标</summary>
        public const int IMAGE_ICON = 1;

        /// <summary>加载标志：从文件加载</summary>
        public const int LR_LOADFROMFILE = 0x00000010;

        /// <summary>加载标志：默认大小</summary>
        public const int LR_DEFAULTSIZE = 0x00000040;

        /// <summary>菜单项类型：分隔符</summary>
        public const int MF_SEPARATOR = 0x0800;
        #endregion

        #endregion
    }
}