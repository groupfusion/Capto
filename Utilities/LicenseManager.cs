using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Capto.Utilities
{
    public class LicenseManager
    {
        private static readonly string LicenseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.lic");
        private static readonly string PublicKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "public.key");
        private static readonly string AppVersion = "v1.0.0";

        public static bool ValidateAndLogLicense()
        {
            try
            {
                if (!File.Exists(LicenseFilePath))
                {
                    LogLicenseResult(false, "许可证文件不存在", null);
                    return false;
                }

                string licenseKey = File.ReadAllText(LicenseFilePath).Trim();
                if (string.IsNullOrEmpty(licenseKey))
                {
                    LogLicenseResult(false, "许可证文件为空", null);
                    return false;
                }

                if (!File.Exists(PublicKeyPath))
                {
                    LogLicenseResult(false, "公钥文件不存在", null);
                    return false;
                }

                string machineId = SystemInfoHelper.GetDeviceId();
                bool isValid = ValidateLicense(licenseKey, machineId, AppVersion);

                var licenseInfo = GetLicenseInfo(licenseKey);
                LogLicenseResult(isValid, isValid ? "许可证有效" : "许可证无效", licenseInfo);

                return isValid;
            }
            catch (Exception ex)
            {
                LogLicenseResult(false, $"验证失败: {ex.Message}", null);
                return false;
            }
        }

        private static void LogLicenseResult(bool isValid, string message, LicenseInfo? info)
        {
            if (isValid)
            {
        
                Capto.Utilities.Logger.Info("许可证验证结果: {Result}", message);
                if (info != null)
                {
                    Capto.Utilities.Logger.Info("设备ID: {MachineId}", info.MachineId);
                    Capto.Utilities.Logger.Info("过期日期: {ExpirationDate}", info.ExpirationDate.ToString("yyyy-MM-dd"));
                    Capto.Utilities.Logger.Info("版本: {Version}", info.Version);
                    Capto.Utilities.Logger.Info("生成时间: {Timestamp}", info.Timestamp);
                }
            }
            else
            {
                Capto.Utilities.Logger.Warning("许可证验证结果: {Result}", message);
                if (info != null)
                {
                    Capto.Utilities.Logger.Warning("设备ID: {MachineId}", info.MachineId);
                    Capto.Utilities.Logger.Warning("过期日期: {ExpirationDate}", info.ExpirationDate.ToString("yyyy-MM-dd"));
                    Capto.Utilities.Logger.Warning("版本: {Version}", info.Version);
                    Capto.Utilities.Logger.Warning("生成时间: {Timestamp}", info.Timestamp);
                }
            }
        }

        public static bool ValidateLicense(string licenseKey, string machineId, string version = "v1.0.0")
        {
            try
            {
                string jsonPackage = Encoding.UTF8.GetString(Convert.FromBase64String(licenseKey));
                var package = JsonSerializer.Deserialize<LicensePackage>(jsonPackage);

                if (package == null || string.IsNullOrEmpty(package.Data) || string.IsNullOrEmpty(package.Signature))
                {
                    return false;
                }

                byte[] dataBytes = Convert.FromBase64String(package.Data);
                byte[] signature = Convert.FromBase64String(package.Signature);

                using var rsa = RSA.Create(4096);
                string publicKey = File.ReadAllText(PublicKeyPath);
                rsa.ImportFromPem(publicKey);

                if (!rsa.VerifyData(dataBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                {
                    return false;
                }

                var licenseData = JsonSerializer.Deserialize<LicenseData>(Encoding.UTF8.GetString(dataBytes));

                if (licenseData == null)
                {
                    return false;
                }

                if (licenseData.MachineId != machineId)
                {
                    return false;
                }

                if (licenseData.Version != version)
                {
                    return false;
                }

                if (!DateTime.TryParse(licenseData.ExpirationDate, out var expirationDate))
                {
                    return false;
                }

                return DateTime.Now <= expirationDate;
            }
            catch
            {
                return false;
            }
        }

        public static LicenseInfo? GetLicenseInfo(string licenseKey)
        {
            try
            {
                string jsonPackage = Encoding.UTF8.GetString(Convert.FromBase64String(licenseKey));
                var package = JsonSerializer.Deserialize<LicensePackage>(jsonPackage);

                if (package == null)
                {
                    Capto.Utilities.Logger.Warning("许可证数据解析失败: 许可证包为空");
                    return null;
                }

                byte[] dataBytes = Convert.FromBase64String(package.Data);
                var licenseData = JsonSerializer.Deserialize<LicenseData>(Encoding.UTF8.GetString(dataBytes));

                if (licenseData == null)
                {
                    Capto.Utilities.Logger.Warning("许可证文件为空");
                    return null;
                }

                return new LicenseInfo
                {
                    MachineId = licenseData.MachineId,
                    ExpirationDate = DateTime.Parse(licenseData.ExpirationDate),
                    Version = licenseData.Version,
                    Timestamp = licenseData.Timestamp
                };
            }
            catch
            {
                return null;
            }
        }

        private class LicenseData
        {
            public string MachineId { get; set; } = "";
            public string ExpirationDate { get; set; } = "";
            public string Version { get; set; } = "";
            public string Timestamp { get; set; } = "";
        }

        private class LicensePackage
        {
            public string Data { get; set; } = "";
            public string Signature { get; set; } = "";
        }

        public class LicenseInfo
        {
            public string MachineId { get; set; } = "";
            public DateTime ExpirationDate { get; set; }
            public string Version { get; set; } = "";
            public string Timestamp { get; set; } = "";
        }
        
        public static string GetLicenseDisplayInfo()
        {
            try
            {
                if (!File.Exists(LicenseFilePath))
                {
                    return "许可证状态: 无许可";
                }
                
                string licenseKey = File.ReadAllText(LicenseFilePath).Trim();
                if (string.IsNullOrEmpty(licenseKey))
                {
                    return "许可证状态: 无许可";
                }
                
                var licenseInfo = GetLicenseInfo(licenseKey);
                if (licenseInfo != null)
                {
                    string machineId = SystemInfoHelper.GetDeviceId();
                    bool isValid = ValidateLicense(licenseKey, machineId, AppVersion);
                    
                    string status = isValid ? "有效" : "无效";
                    
                    return $"许可证状态: {status} 到期时间: {licenseInfo.ExpirationDate:yyyy-MM-dd}";
                }
                return "许可证状态: 无许可";
            }
            catch (Exception ex)
            {
                Capto.Utilities.Logger.Warning($"许可证状态: 获取失败\n{ex.Message}");
                return $"许可证状态: 无许可";
            }
        }
    }
}
