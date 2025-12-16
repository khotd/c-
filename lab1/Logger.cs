using System;
using System.IO;

public static class Logger
{
    private static readonly object _lock = new object();
    private const string LogFilePath = "errors.log";
    
    public static void LogError(string message)
    {
        lock (_lock)
        {
            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}";
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch
            {
                // игнорируем, если не записалось в лог
            }
        }
    }
}