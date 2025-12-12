using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("🏴‍☠️ BarrerOS Phase 2.9 - C# Init + Shell");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();
Console.WriteLine("✅ .NET 10 Runtime Loaded");
Console.WriteLine("✅ C# Code Executing on Real Filesystem");
Console.WriteLine("✅ Init Process Running as PID 1");
Console.WriteLine();
Console.WriteLine("💙 Captain CP & Daniel Elliott");
Console.WriteLine("📅 December 12, 2025 - BarrerOS Lives!");
Console.WriteLine();
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();

// Test C# coreutils
Console.WriteLine("🧪 Testing C# coreutils...");
Console.WriteLine();

Console.WriteLine("Running: pwd");
var pwd = Process.Start("/bin/pwd", "");
pwd?.WaitForExit();
Console.WriteLine();

Console.WriteLine("Running: echo Hello from BarrerOS!");
var echo = Process.Start("/bin/echo", "Hello from BarrerOS!");
echo?.WaitForExit();
Console.WriteLine();

Console.WriteLine("Running: ls /");
var ls = Process.Start("/bin/ls", "/");
ls?.WaitForExit();
Console.WriteLine();

Console.WriteLine("Running: mkdir /tmp/test");
var mkdir = Process.Start("/bin/mkdir", "/tmp/test");
mkdir?.WaitForExit();

Console.WriteLine("Running: touch /tmp/test/hello.txt");
var touch = Process.Start("/bin/touch", "/tmp/test/hello.txt");
touch?.WaitForExit();

Console.WriteLine("Running: echo 'BarrerOS works!' | tee would write but we'll use cp");
var echo2 = Process.Start("/bin/echo", "BarrerOS works!");
echo2?.WaitForExit();

Console.WriteLine("Running: ls /tmp/test");
var lstmp = Process.Start("/bin/ls", "/tmp/test");
lstmp?.WaitForExit();

Console.WriteLine("Running: touch /tmp/test/file2.txt");
var touch2 = Process.Start("/bin/touch", "/tmp/test/file2.txt");
touch2?.WaitForExit();

Console.WriteLine("Running: mv /tmp/test/file2.txt /tmp/test/renamed.txt");
var mv = Process.Start("/bin/mv", "/tmp/test/file2.txt /tmp/test/renamed.txt");
mv?.WaitForExit();

Console.WriteLine("Running: ls /tmp/test (after mv)");
var lstmp2 = Process.Start("/bin/ls", "/tmp/test");
lstmp2?.WaitForExit();

Console.WriteLine("Running: ps");
var pscmd = Process.Start("/bin/ps", "");
pscmd?.WaitForExit();
Console.WriteLine();

Console.WriteLine("Running: grep 'tmp' on ls output (simulated - testing grep on file)");
Console.WriteLine("Creating test file...");
File.WriteAllText("/tmp/greptest.txt", "line1\ntmpfile here\nline3\nanother tmp line");
var grepcmd = Process.Start("/bin/grep", "tmp /tmp/greptest.txt");
grepcmd?.WaitForExit();
Console.WriteLine();

Console.WriteLine("Running: chmod 755 /tmp/test");
var chmodcmd = Process.Start("/bin/chmod", "755 /tmp/test");
chmodcmd?.WaitForExit();
Console.WriteLine();

Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("🔄 Entering event loop... (PID 1 will never exit)");
Console.WriteLine();

// Track uptime
var startTime = DateTime.UtcNow;
int cycleCount = 0;

// Helper to read memory info
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

// Main event loop - PID 1 must NEVER exit
while (true)
{
    cycleCount++;
    
    // Every 60 seconds, show we're alive with memory stats
    if (cycleCount % 600 == 0)
    {
        var uptime = DateTime.UtcNow - startTime;
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] Init alive - Uptime: {uptime.TotalSeconds:F0}s");
        ShowMemoryInfo();
    }
    
    // TODO Phase 2.10: Handle signals (SIGCHLD for zombie reaping)
    // TODO Phase 2.11: Service management
    
    // Sleep 100ms between cycles
    Thread.Sleep(100);
}
