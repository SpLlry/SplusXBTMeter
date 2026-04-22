using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public static class Win32Api
{
    #region 基础窗口 API
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    #endregion

    #region 托盘专用 API
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT pt);

    [DllImport("user32.dll")]
    public static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

    [DllImport("user32.dll")]
    public static extern int TrackPopupMenu(IntPtr hMenu, int uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32.dll")]
    public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    public static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    public static extern IntPtr DispatchMessage(ref MSG lpMsg);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    [DllImport("user32.dll")]
    public static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    #endregion

    #region 结构体
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct NOTIFYICONDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public int dwState;
        public int dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public int uTimeout;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public int dwInfoFlags;
        public Guid guidItem;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }
    #endregion

    #region 常量
    public const int GWL_STYLE = -16;
    public const int GWL_EXSTYLE = -20;
    public const int GWL_WNDPROC = -4;

    public const uint WS_CHILD = 0x40000000;
    public const uint WS_VISIBLE = 0x10000000;
    public const int WS_POPUP = unchecked((int)0x80000000);
    public const int SW_HIDE = 0;

    public const uint WS_EX_TRANSPARENT = 0x00000020;
    public const uint WS_EX_LAYERED = 0x00080000;

    public const uint SWP_NOZORDER = 0x0004;
    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint SWP_FRAMECHANGED = 0x0020;
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOSIZE = 0x0001;

    public const int WM_WINDOWPOSCHANGED = 0x0047;
    public const int WM_DESTROY = 0x0002;
    public const int WM_TRAY_MSG = 0x400;
    public const int WM_RBUTTONUP = 0x0205;

    public const int NIM_ADD = 0x00000000;
    public const int NIM_DELETE = 0x00000002;
    public const int NIF_ICON = 0x00000002;
    public const int NIF_TIP = 0x00000004;
    public const int NIF_MESSAGE = 0x00000001;

    public const int TPM_RETURNCMD = 0x00000100;
    public static readonly IntPtr IDI_APPLICATION = new IntPtr(32512);

    // 菜单分隔符 🔥 新增
    public const int MF_SEPARATOR = 0x0800;
    #endregion

    #region 工具方法
    public static IntPtr GetWpfWindowHwnd(Window window)
    {
        return new WindowInteropHelper(window).EnsureHandle();
    }

    public static IntPtr FindTaskbarEmbedContainer()
    {
        IntPtr hTaskbar = FindWindow("Shell_TrayWnd", "");
        Debug.WriteLine($"找到任务栏主窗口句柄：{hTaskbar}");
        Debug.WriteLine(hTaskbar == IntPtr.Zero);
        if (hTaskbar == IntPtr.Zero) return IntPtr.Zero;
        return hTaskbar;
        if (Environment.OSVersion.Version.Build >= 22000)
        {
            IntPtr hTaskbarContent = FindWindowEx(hTaskbar, IntPtr.Zero, "TaskbarContentView", null);
            if (hTaskbarContent != IntPtr.Zero)
            {
                IntPtr hPrimaryContent = FindWindowEx(hTaskbarContent, IntPtr.Zero, "PrimaryContent", null);
                if (hPrimaryContent != IntPtr.Zero)
                {
                    return FindWindowEx(hPrimaryContent, IntPtr.Zero, "MSTaskListWClass", null);
                }
            }
        }

        IntPtr hReBar = FindWindowEx(hTaskbar, IntPtr.Zero, "ReBarWindow32", null);
        if (hReBar != IntPtr.Zero)
        {
            IntPtr hTaskBand = FindWindowEx(hReBar, IntPtr.Zero, "MSTaskSwWClass", null);
            if (hTaskBand != IntPtr.Zero)
            {
                return hTaskBand;
            }
        }

        return FindWindowEx(hTaskbar, IntPtr.Zero, "TrayNotifyWnd", null);
    }
    #endregion
}