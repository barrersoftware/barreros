using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

// Mount essential filesystems first
MountFilesystems();

Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("🏴‍☠️ BarrerOS Init System v3.0");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();
Console.WriteLine("✅ .NET 10 Runtime Loaded");
Console.WriteLine("✅ C# Init Running as PID 1");
Console.WriteLine();
Console.WriteLine("💙 Captain CP & Daniel Elliott");
Console.WriteLine("📅 December 16, 2025");
Console.WriteLine();
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();

// Service definitions
var services = new List<ServiceInfo>
{
    new ServiceInfo
    {
        Name = "barrerd-log",
        Path = "/sbin/barrerd-log",
        Description = "Logging Service",
        Required = true
    },
    new ServiceInfo
    {
        Name = "barrerd-devmgr",
        Path = "/sbin/barrerd-devmgr",
        Description = "Device Manager",
        Required = true
    },
    new ServiceInfo
    {
        Name = "barrerd-net",
        Path = "/sbin/barrerd-net",
        Description = "Network Manager",
        Required = false
    }
};

var runningServices = new Dictionary<string, Process>();
var startTime = DateTime.UtcNow;
var shutdownRequested = false;

// Start all services
Console.WriteLine("🚀 Starting system services...");
Console.WriteLine();

foreach (var service in services)
{
    if (File.Exists(service.Path))
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = service.Path,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };
            
            var process = Process.Start(psi);
            if (process != null)
            {
                runningServices[service.Name] = process;
                Console.WriteLine($"✓ Started {service.Description} (PID {process.Id})");
            }
            else
            {
                Console.WriteLine($"✗ Failed to start {service.Description}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error starting {service.Description}: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine($"⚠ Service not found: {service.Path}");
    }
}

Console.WriteLine();
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("✅ BarrerOS boot complete!");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();

// Monitor services
int cycleCount = 0;

while (!shutdownRequested)
{
    cycleCount++;
    
    // Check service health every 10 seconds
    if (cycleCount % 100 == 0)
    {
        var uptime = DateTime.UtcNow - startTime;
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] System uptime: {uptime.TotalSeconds:F0}s");
        
        // Check each service
        foreach (var service in services)
        {
            if (runningServices.TryGetValue(service.Name, out var process))
            {
                if (process.HasExited)
                {
                    Console.WriteLine($"⚠ Service {service.Name} exited with code {process.ExitCode}");
                    
                    // Restart if required
                    if (service.Required)
                    {
                        Console.WriteLine($"🔄 Restarting {service.Name}...");
                        try
                        {
                            var psi = new ProcessStartInfo
                            {
                                FileName = service.Path,
                                UseShellExecute = false,
                                RedirectStandardOutput = false,
                                RedirectStandardError = false
                            };
                            
                            var newProcess = Process.Start(psi);
                            if (newProcess != null)
                            {
                                runningServices[service.Name] = newProcess;
                                Console.WriteLine($"✓ Restarted {service.Name} (PID {newProcess.Id})");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"✗ Failed to restart {service.Name}: {ex.Message}");
                        }
                    }
                }
            }
        }
        
        ShowMemoryInfo();
        Console.WriteLine();
    }
    
    // Sleep 100ms between cycles
    Thread.Sleep(100);
}

// Shutdown (if ever reached)
Console.WriteLine();
Console.WriteLine("🛑 Shutting down...");

foreach (var kvp in runningServices)
{
    try
    {
        if (!kvp.Value.HasExited)
        {
            Console.WriteLine($"Stopping {kvp.Key}...");
            kvp.Value.Kill();
            kvp.Value.WaitForExit(5000);
        }
    }
    catch { }
}

Console.WriteLine("Shutdown complete");
return 0;

// Helper functions
[DllImport("libc", SetLastError = true)]
static extern int mount(string source, string target, string filesystemtype, ulong mountflags, string data);

static void MountFilesystems()
{
    Console.WriteLine("🔧 Mounting essential filesystems...");
    
    if (!Directory.Exists("/proc"))
        Directory.CreateDirectory("/proc");
    if (mount("proc", "/proc", "proc", 0, null) == 0)
        Console.WriteLine("✓ Mounted /proc");
    else
        Console.WriteLine("⚠ Failed to mount /proc (may already be mounted)");
    
    if (!Directory.Exists("/sys"))
        Directory.CreateDirectory("/sys");
    if (mount("sysfs", "/sys", "sysfs", 0, null) == 0)
        Console.WriteLine("✓ Mounted /sys");
    else
        Console.WriteLine("⚠ Failed to mount /sys (may already be mounted)");
    
    if (!Directory.Exists("/dev"))
        Directory.CreateDirectory("/dev");
    if (mount("devtmpfs", "/dev", "devtmpfs", 0, null) == 0)
        Console.WriteLine("✓ Mounted /dev");
    else
        Console.WriteLine("⚠ Failed to mount /dev (may already be mounted)");
    
    Console.WriteLine();
}

static void ShowMemoryInfo()
{
    try
    {
        var lines = File.ReadAllLines("/proc/meminfo");
        var memTotal = 0L;
        var memFree = 0L;
        var memAvailable = 0L;
        
        foreach (var line in lines)
        {
            if (line.StartsWith("MemTotal:"))
                memTotal = long.Parse(line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
            else if (line.StartsWith("MemFree:"))
                memFree = long.Parse(line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
            else if (line.StartsWith("MemAvailable:"))
                memAvailable = long.Parse(line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
        }
        
        var memUsed = memTotal - memFree;
        var totalMB = memTotal / 1024;
        var usedMB = memUsed / 1024;
        var availMB = memAvailable / 1024;
        
        Console.WriteLine($"    Memory: {usedMB}MB used / {totalMB}MB total ({availMB}MB available)");
    }
    catch { }
}

class ServiceInfo
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required string Description { get; init; }
    public bool Required { get; init; }
}
