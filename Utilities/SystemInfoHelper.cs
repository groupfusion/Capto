using System;
using System.Management;
using System.Text;
using System.Security.Cryptography;
using System.Reflection;
using System.Windows.Forms;

namespace Capto.Utilities
{
    public static class SystemInfoHelper
    {
        public static string CollectSystemInfo()
        {
            var sb = new StringBuilder();
            
            sb.Append($"CPU: {GetCpuId()}");
            sb.Append($"|硬盘: {GetHardDriveId()}");
            sb.Append($"|网卡: {GetNetworkAdapterId()}");
            
            return sb.ToString();
        }
        
        public static string GetDeviceId()
        {
            string systemInfo = CollectSystemInfo();
            return GenerateMd5(systemInfo);
        }
        
        public static string GetDetailedSystemInfo()
        {
            var sb = new StringBuilder();
            
            // 应用信息
            sb.AppendLine($"应用版本: {GetAppVersion()}");
            // 操作系统信息
            sb.AppendLine($"操作系统: {GetOSVersion()}");
            sb.AppendLine($".NET版本: {GetDotNetVersion()}");
            sb.AppendLine($"设备ID: {GetDeviceId()}");
            return sb.ToString();
        }
        
        private static string GenerateMd5(string input)
        {
            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
        
        private static string GetAppVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
            {
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
            return "1.0.0";
        }
        
        private static string GetOSVersion()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Caption, Version, OSArchitecture FROM Win32_OperatingSystem");
                foreach (var obj in searcher.Get())
                {
                    string caption = obj["Caption"]?.ToString() ?? "未知";
                    string version = obj["Version"]?.ToString() ?? "未知";
                    string architecture = obj["OSArchitecture"]?.ToString() ?? "未知";
                    return $"{caption} {version} ({architecture})";
                }
            }
            catch { }
            return "未知";
        }
        
        private static string GetDotNetVersion()
        {
            try
            {
                return Environment.Version.ToString();
            }
            catch { }
            return "未知";
        }
        
        private static string GetScreenResolution()
        {
            try
            {
                int width = Screen.PrimaryScreen?.Bounds.Width ?? 0;
                int height = Screen.PrimaryScreen?.Bounds.Height ?? 0;
                return $"{width}x{height}";
            }
            catch { }
            return "未知";
        }
        
        private static string GetSystemMemory()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
                foreach (var obj in searcher.Get())
                {
                    if (obj["TotalVisibleMemorySize"] is ulong memorySize)
                    {
                        // 转换为GB
                        double memoryInGB = memorySize / 1024.0 / 1024.0;
                        return memoryInGB.ToString("F2");
                    }
                }
            }
            catch { }
            return "未知";
        }
        
        private static string GetCpuId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (var obj in searcher.Get())
                {
                    return obj["ProcessorId"]?.ToString() ?? "未知";
                }
            }
            catch { }
            return "未知";
        }
        
        private static string GetHardDriveId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive");
                foreach (var obj in searcher.Get())
                {
                    return obj["SerialNumber"]?.ToString()?.Trim() ?? "未知";
                }
            }
            catch { }
            return "未知";
        }
        
        private static string GetNetworkAdapterId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT MACAddress FROM Win32_NetworkAdapter WHERE PhysicalAdapter = True");
                foreach (var obj in searcher.Get())
                {
                    return obj["MACAddress"]?.ToString() ?? "未知";
                }
            }
            catch { }
            return "未知";
        }
    }
}
