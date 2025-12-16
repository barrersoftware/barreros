using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

// BarrerOS Logging Service (barrerd-log)
// Replaces syslog - captures kernel messages and application logs

namespace BarrerOS.Services.Logging;

class LogService
{
    private const string KernelLogPath = "/proc/kmsg";
    private const string LogDirectory = "/var/log/barreros";
    private const string SystemLogFile = "/var/log/barreros/system.log";
    private const string KernelLogFile = "/var/log/barreros/kernel.log";
    
    private static readonly ConcurrentQueue<LogEntry> _logQueue = new();
    private static readonly CancellationTokenSource _cts = new();
    private static StreamWriter? _systemLog;
    private static StreamWriter? _kernelLog;

    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("📝 BarrerOS Logging Service v1.0");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine();

        // Ensure log directory exists
        if (!Directory.Exists(LogDirectory))
        {
            Directory.CreateDirectory(LogDirectory);
            Console.WriteLine($"Created log directory: {LogDirectory}");
        }

        // Open log files
        try
        {
            _systemLog = new StreamWriter(SystemLogFile, append: true) { AutoFlush = true };
            _kernelLog = new StreamWriter(KernelLogFile, append: true) { AutoFlush = true };
            Console.WriteLine($"System log: {SystemLogFile}");
            Console.WriteLine($"Kernel log: {KernelLogFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to open log files: {ex.Message}");
            return 1;
        }

        Console.WriteLine();
        Console.WriteLine("Starting logging services...");
        Console.WriteLine();

        // Handle Ctrl+C gracefully
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            _cts.Cancel();
        };

        // Start background tasks
        var kernelTask = Task.Run(() => ReadKernelLog(_cts.Token));
        var writerTask = Task.Run(() => WriteLogQueue(_cts.Token));

        Console.WriteLine("✅ Logging service started");
        Console.WriteLine("   - Kernel log reader: active");
        Console.WriteLine("   - Log writer: active");
        Console.WriteLine();
        Console.WriteLine("Press Ctrl+C to stop");
        Console.WriteLine();

        // Wait for cancellation
        await Task.WhenAll(kernelTask, writerTask);

        // Cleanup
        _systemLog?.Close();
        _kernelLog?.Close();

        Console.WriteLine();
        Console.WriteLine("Logging service stopped");
        return 0;
    }

    static async Task ReadKernelLog(CancellationToken token)
    {
        try
        {
            if (!File.Exists(KernelLogPath))
            {
                LogToConsole("WARNING", $"Kernel log not available: {KernelLogPath}");
                return;
            }

            using var stream = new FileStream(KernelLogPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);

            while (!token.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(token);
                if (line != null)
                {
                    _logQueue.Enqueue(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        Level = "KERNEL",
                        Message = line,
                        Source = "kernel"
                    });
                }
                else
                {
                    await Task.Delay(100, token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            LogToConsole("ERROR", $"Kernel log reader failed: {ex.Message}");
        }
    }

    static async Task WriteLogQueue(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                if (_logQueue.TryDequeue(out var entry))
                {
                    var logLine = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.Level}] {entry.Message}";

                    // Write to appropriate log file
                    if (entry.Source == "kernel" && _kernelLog != null)
                    {
                        await _kernelLog.WriteLineAsync(logLine);
                    }
                    else if (_systemLog != null)
                    {
                        await _systemLog.WriteLineAsync(logLine);
                    }

                    // Also output to console
                    Console.WriteLine(logLine);
                }
                else
                {
                    await Task.Delay(50, token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            LogToConsole("ERROR", $"Log writer failed: {ex.Message}");
        }
    }

    static void LogToConsole(string level, string message)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
    }

    public static void Log(string level, string message, string source = "system")
    {
        _logQueue.Enqueue(new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Message = message,
            Source = source
        });
    }
}

record LogEntry
{
    public DateTime Timestamp { get; init; }
    public required string Level { get; init; }
    public required string Message { get; init; }
    public string Source { get; init; } = "system";
}
