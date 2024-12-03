using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace FaceCheck.Server.Helper
{
    /// <summary>
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static partial class Win32
    {
        /// <summary>
        /// 命令行程序退出回调
        /// </summary>
        /// <param name="dwCtrlType"></param>
        /// <returns></returns>
        public delegate bool ConsoleCtrlDelegate(int dwCtrlType);

        /// <summary>
        ///
        /// </summary>
        public const int STD_INPUT_HANDLE = -10;

        /// <summary>
        ///
        /// </summary>
        public const uint ENABLE_QUICK_EDIT_MODE = 0x0040;

        /// <summary>
        /// 隐藏
        /// </summary>
        public const int SW_HIDE = 0;

        /// <summary>
        /// 命令行程序退出回调
        /// </summary>
        /// <![CDATA[
        /// Win32Helper.ConsoleCtrlDelegate newDelegate = new Win32Helper.ConsoleCtrlDelegate(HandlerRoutine);
        /// if (!Win32Helper.SetConsoleCtrlHandler(newDelegate, true))
        /// {
        ///     Console.WriteLine("抱歉，API注入失败，按任意键退出！");
        ///     return;
        /// }
        ///
        /// private static bool HandlerRoutine(int CtrlType)
        /// {
        ///     // 退出回调
        ///     Console.WriteLine($"HandlerRoutine: {CtrlType}");
        ///     return true;
        /// }
        /// ]]>
        /// <param name="HandlerRoutine"></param>
        /// <param name="Add"></param>
        /// <returns></returns>
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, [MarshalAs(UnmanagedType.Bool)] bool Add);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        internal static partial IntPtr GetStdHandle(int hConsoleHandle);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint mode);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint mode);

        /// <summary>
        /// 获取命令行窗口句柄
        /// </summary>
        /// <![CDATA[
        /// IntPtr intptr = Win32Helper.GetConsoleWindow();
        /// if (intptr != IntPtr.Zero)
        /// {
        ///     Win32Helper.ShowWindow(intptr, Win32Helper.SW_HIDE);
        /// }
        /// ]]>
        /// <returns></returns>
        [LibraryImport("kernel32.dll")]
        public static partial IntPtr GetConsoleWindow();

        /// <summary>
        /// 用于隐藏窗口
        /// </summary>
        /// <![CDATA[
        /// IntPtr intptr = Win32Helper.GetConsoleWindow();
        /// if (intptr != IntPtr.Zero)
        /// {
        ///     Win32Helper.ShowWindow(intptr, Win32Helper.SW_HIDE);
        /// }
        /// ]]>
        /// <param name="hWnd"></param>
        /// <param name="nCmdShow"></param>
        /// <returns></returns>
        [LibraryImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        /// <summary>
        /// 命令行程序禁用光标
        /// </summary>
        /// <![CDATA[
        /// Win32Helper.DisbleQuickEditMode();
        /// ]]>
        public static void DisbleQuickEditMode()
        {
            IntPtr hStdin = GetStdHandle(STD_INPUT_HANDLE);
            GetConsoleMode(hStdin, out uint mode);
            mode &= ~ENABLE_QUICK_EDIT_MODE;
            SetConsoleMode(hStdin, mode);
        }
    }
}