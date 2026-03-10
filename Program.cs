using System;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Capto.Utilities;

namespace Capto
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private static Mutex? _instanceMutex;

        [STAThread]
        static void Main()
        {
            try
            {
                bool createdNew;
                _instanceMutex = new Mutex(true, "Capto_SingleInstance_Mutex", out createdNew);

                if (!createdNew)
                {
                    MessageBox.Show("Capto 已在运行中，无法重复启动。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Logger.Initialize();

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
                Logger.Error($"应用程序启动失败: {ex.Message}", ex);
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }
            finally
            {
                Logger.Shutdown();
                _instanceMutex?.ReleaseMutex();
                _instanceMutex?.Dispose();
            }
        }
    }
}