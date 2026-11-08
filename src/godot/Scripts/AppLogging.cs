using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace godot.Scripts
{
    public static class AppLogging
    {
        private static readonly NLogLoggerProvider LoggerProvider = new NLogLoggerProvider(
            new NLogProviderOptions { ShutdownOnDispose = false });

        public static Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return LoggerProvider.CreateLogger(categoryName);
        }

        public static void Configure(string directory, LogLevel minimumLevel = null)
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentException("A log directory is required.", nameof(directory));

            string logDirectory = Path.GetFullPath(directory);
            Directory.CreateDirectory(logDirectory);

            var file = new FileTarget("logFile")
            {
                FileName = Path.Combine(logDirectory, "wma-${shortdate}.log"),
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}${onexception:inner=${newline}${exception:format=tostring}}",
                KeepFileOpen = true
            };
            var asyncFile = new AsyncTargetWrapper(file)
            {
                Name = "asyncLogFile",
                QueueLimit = 10000,
                // Keep producers non-blocking without dropping entries on queue overflow.
                OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
                BatchSize = 200,
                TimeToSleepBetweenBatches = 1
            };
            var configuration = new LoggingConfiguration();
            configuration.AddRule(minimumLevel ?? LogLevel.Info, LogLevel.Fatal, asyncFile);
            LogManager.Configuration = configuration;
        }

        public static void Shutdown()
        {
            try
            {
                LogManager.Flush(TimeSpan.FromSeconds(5));
            }
            finally
            {
                LogManager.Shutdown();
            }
        }
    }
}
