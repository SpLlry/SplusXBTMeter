using System;
using System.Runtime.InteropServices;
using System.Windows;
using HandyControl.Controls;

namespace SplusXBTMeter
{
    public partial class TrayWindow : HandyControl.Controls.Window
    {
        #region Win32 API 完整声明
        private const int WM_TRAY_MSG = 0x400;
        private const int WM_RBUTTONUP = 0x0205;
        private const int NIM_ADD = 0x00000000;
        private const int NIM_DELETE = 0x00000002;
        private const int NIF_ICON = 0x00000002;
        private const int NIF_TIP = 0x00000004;
        private const int NIF_MESSAGE = 0x00000001;
        private const int TPM_RETURNCMD = 0x00000100;
        private const int GWL_WNDPROC = -4;
        private const int WS_POPUP = unchecked((int)0x80000000);
        private const int SW_HIDE = 0;

        private static IntPtr _originalWndProcPtr;

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA lpData);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NOTIFYICONDATA
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
        #endregion

        public TrayWindow()
        {
            InitializeComponent();
            // 隐藏当前载体窗口（仅作为托盘宿主）
           
            IntPtr wndProcPtr = Marshal.GetFunctionPointerForDelegate(new WndProc(TrayWindowProc));
        }

        private static IntPtr TrayWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            // 监听托盘右键消息
            if (msg == WM_TRAY_MSG && lParam == (IntPtr)WM_RBUTTONUP)
            {
               Console.WriteLine("右键触发成功！");
                return IntPtr.Zero;
            }
            // 调用原始窗口过程
            return CallWindowProc(_originalWndProcPtr, hWnd, msg, wParam, lParam);
        }

        // 窗口过程委托
        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    }
}