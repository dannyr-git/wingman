using System;
using System.IO;
using System.Runtime.CompilerServices;
using wingman.Interfaces;
using wingman.Services;

namespace wingman.Interfaces
{
    public interface ILoggingService
    {
        event EventHandler<string> UIOutputHandler;

        void LogInfo(string message, bool includeReflectionInfo = false, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
        void LogDebug(string message, bool includeReflectionInfo = true, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
        void LogWarning(string message, bool includeReflectionInfo = true, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
        void LogError(string message, bool includeReflectionInfo = true, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
        void LogException(string message, bool includeReflectionInfo = true, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
        void LogApiQuery(string prompt, string response);

        void SetVerboseLevel(VerboseLevel level);
    }
}

namespace wingman.Services
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string MemberName { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string Prompt { get; set; }
        public string Response { get; set; }
    }

    public enum VerboseLevel
    {
        Verbose,
        Normal
    }

    public class LoggingService : ILoggingService
    {
        private readonly object _lock = new object();
        private readonly string _logFile;
        private string _logBook = $"Welcome to Wingman!\r\n\r\n";
        private VerboseLevel _verboseLevel = VerboseLevel.Normal;

        public event EventHandler<string> UIOutputHandler;

        public LoggingService()
        {
            _logBook += $"\r\n";
            _logBook += $"1. Make sure you enter a valid OpenAI key.  I can only validate format, not validity.\r\n";
            _logBook += $"2. Press and hold your hotkey to record, let go when you're done speaking.\r\n";
            _logBook += $"3. Main+Clipboard and Modal+Clipboard means whatever is on your clipboard will be appended to the voice prompt you are sending\r\n";
            _logBook += $"4. To configure a hotkey, click the button + hit a hotkey\r\n";
            _logBook += $"GIT: https://github.com/dannyr-git/wingman\r\n";
            _logBook += $"\r\nPlease remember, we're still in Pre-Release!\r\n";
            _logBook += $"\r\n>>> Log Start: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n";

            string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Wingman");
            Directory.CreateDirectory(logDirectory);
            _logFile = Path.Combine(logDirectory, $"Wingman{DateTime.Now:yyyyMMdd_HHmmss}.log");
        }

        public void LogDebug(string message, bool includeReflectionInfo = true, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log("DEBUG", message, includeReflectionInfo, memberName, filePath, lineNumber);
        }

        public void LogInfo(string message, bool includeReflectionInfo = false, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log("INFO", message, includeReflectionInfo, memberName, filePath, lineNumber);
        }

        public void LogWarning(string message, bool includeReflectionInfo = true, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log("WARNING", message, includeReflectionInfo, memberName, filePath, lineNumber);
        }

        public void LogError(string message, bool includeReflectionInfo = true, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log("ERROR", message, includeReflectionInfo, memberName, filePath, lineNumber);
        }

        public void LogException(string message, bool includeReflectionInfo = true, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log("EXCEPTION", message, includeReflectionInfo, memberName, filePath, lineNumber);
        }

        public void LogApiQuery(string prompt, string response)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = "API",
                Prompt = prompt,
                Response = response
            };

            WriteLogEntry(logEntry);
            UIOutputHandler?.Invoke(this, _logBook);
        }

        public void SetVerboseLevel(VerboseLevel level)
        {
            _verboseLevel = level;
        }

        private void Log(string level, string message, bool includeReflectionInfo, string memberName, string filePath, int lineNumber)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message
            };

            if (includeReflectionInfo)
            {
                logEntry.MemberName = memberName;
                logEntry.FilePath = filePath;
                logEntry.LineNumber = lineNumber;
            }

            WriteLogEntry(logEntry);

            UIOutputHandler?.Invoke(this, _logBook);
        }

        private void WriteLogEntry(LogEntry logEntry)
        {
            string formattedMessage = FormatLogEntry(logEntry);

            lock (_lock)
            {
                File.AppendAllText(_logFile, formattedMessage + Environment.NewLine);

                if (_verboseLevel == VerboseLevel.Verbose)
                    _logBook += formattedMessage + Environment.NewLine;
                else if (logEntry.Level != "DEBUG" && logEntry.Level != "EXCEPTION")
                    _logBook += $"{logEntry.Timestamp:HH:mm:ss} [{logEntry.Level}] {logEntry.Message}" + Environment.NewLine;
            }
        }

        private string FormatLogEntry(LogEntry logEntry)
        {
            if (logEntry.Level == "API")
            {
                return $"{logEntry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{logEntry.Level}] Prompt: {logEntry.Prompt} | Response: {logEntry.Response}";
            }

            string reflectionInfo = string.Empty;

            if (!string.IsNullOrEmpty(logEntry.MemberName) && !string.IsNullOrEmpty(logEntry.FilePath))
            {
                reflectionInfo = $" {Path.GetFileName(logEntry.FilePath)}:{logEntry.LineNumber} {logEntry.MemberName} -";
            }

            return $"{logEntry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{logEntry.Level}]{reflectionInfo} {logEntry.Message}";
        }
    }
}