using System;
using System.IO;
using Serilog;

namespace Capto.Utilities
{
    public static class Logger
    {
        private static ILogger? _logger;
        
        public static void Initialize()
        {
            try
            {
                string logDirectory = Path.Combine(
#if DEBUG
                    AppDomain.CurrentDomain.BaseDirectory,
                    "logs"
#else
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Capto",
                    "logs"
#endif
                );
                
                Directory.CreateDirectory(logDirectory);
                
                string logPath = Path.Combine(logDirectory, "capto-.log");
                
                _logger = new LoggerConfiguration()
                #if DEBUG
                    .MinimumLevel.Debug()
                #else
                    .MinimumLevel.Information()
                #endif
                    .WriteTo.Console()
                    .WriteTo.File(logPath, 
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();
                
                _logger.Information("=== Capto Application Started ===");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[FATAL] Logger initialization failed: {ex.Message}");
                
                try
                {
                    _logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.Console()
                        .CreateLogger();
                }
                catch
                {
                    _logger = null;
                }
            }
        }
        
        public static void Debug(string message)
        {
            if (_logger == null)
            {
                Console.Error.WriteLine($"[DEBUG] {message}");
                return;
            }
            _logger.Debug(message);
        }
        
        public static void Info(string message)
        {
            if (_logger == null)
            {
                Console.Error.WriteLine($"[INFO] {message}");
                return;
            }
            _logger.Information(message);
        }
        
        public static void Warning(string message)
        {
            if (_logger == null)
            {
                Console.Error.WriteLine($"[WARN] {message}");
                return;
            }
            _logger.Warning(message);
        }
        
        public static void Error(string message, Exception? ex = null)
        {
            if (_logger == null)
            {
                Console.Error.WriteLine($"[ERROR] {message}");
                if (ex != null)
                    Console.Error.WriteLine(ex.ToString());
                return;
            }
            
            if (ex != null)
                _logger.Error(ex, message);
            else
                _logger.Error(message);
        }
        
        public static void Shutdown()
        {
            if (_logger != null)
            {
                _logger.Information("=== Capto Application Shutdown ===");
            }
            Log.CloseAndFlush();
        }
    }
}
