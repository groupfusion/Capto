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
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Capto",
                    "logs"
                );
                
                Directory.CreateDirectory(logDirectory);
                
                string logPath = Path.Combine(logDirectory, "capto-.log");
                
                _logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.File(logPath, 
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();
            }
            catch
            {
                _logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .CreateLogger();
            }
            
            _logger.Information("=== Capto Application Started ===");
        }
        
        public static void Debug(string message) => _logger?.Debug(message);
        public static void Info(string message) => _logger?.Information(message);
        public static void Warning(string message) => _logger?.Warning(message);
        public static void Error(string message, Exception? ex = null)
        {
            if (ex != null)
                _logger?.Error(ex, message);
            else
                _logger?.Error(message);
        }
        
        public static void Shutdown()
        {
            _logger?.Information("=== Capto Application Shutdown ===");
            Log.CloseAndFlush();
        }
    }
}
