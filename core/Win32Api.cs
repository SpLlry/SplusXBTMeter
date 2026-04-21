using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public static class Win32Api
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    //查找窗口句柄
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

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public const int GWL_STYLE = -16;
    public const int GWL_EXSTYLE = -20;
    public const int GWL_WNDPROC = -4;
    public const uint WS_CHILD = 0x40000000;
    public const uint WS_VISIBLE = 0x10000000;
    public const uint WS_EX_TRANSPARENT = 0x00000020;
    public const uint WS_EX_LAYERED = 0x00080000;
    public const uint SWP_NOZORDER = 0x0004;
    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint SWP_FRAMECHANGED = 0x0020;
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOSIZE = 0x0001;
    public const int WM_WINDOWPOSCHANGED = 0x0047;
    public const int WM_DESTROY = 0x0002;

    public static IntPtr GetWpfWindowHwnd(Window window)
    {
        return new WindowInteropHelper(window).EnsureHandle();
    }

    // 核心：遍历任务栏窗口树，找到正确的嵌入容器（Win10/11兼容）
    public static IntPtr FindTaskbarEmbedContainer()
    {
        IntPtr hTaskbar = FindWindow("Shell_TrayWnd", "");
        return hTaskbar;
        Debug.WriteLine($"找到任务栏主窗口句柄：{hTaskbar}");
        Debug.WriteLine(hTaskbar == IntPtr.Zero);
        if (hTaskbar == IntPtr.Zero) return IntPtr.Zero;

        // Win11优先查找任务栏内容容器
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

        // Win10兼容路径
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
}