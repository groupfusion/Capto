using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Capto
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main()
        {
            try
            {
                // 设置DPI感知，确保应用程序能够正确处理高DPI显示
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    SetProcessDPIAware();
                }
                // 初始化Windows Forms
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                // 创建TrayIcon（在主线程创建，避免被垃圾回收）
                var trayIcon = new TrayIcon();
                
                // 显示应用程序
                Application.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"应用程序启动失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }
        }
    }
}