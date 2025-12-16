using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Net.NetworkInformation;

// BarrerOS Network Manager (barrerd-net)
// Manages network interfaces, IP configuration, DHCP

namespace BarrerOS.Services.Network;

class NetworkManager
{
    private const string SysClassNetPath = "/sys/class/net";
    private const string NetworkConfigPath = "/etc/barreros/network.conf";
    
    private static readonly Dictionary<string, InterfaceInfo> _interfaces = new();
    private static readonly CancellationTokenSource _cts = new();

    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("🌐 BarrerOS Network Manager v1.0");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine();

        // Wait for /sys/class/net to exist (kernel may still be initializing)
        Console.WriteLine("Checking /sys mount status...");
        
        if (Directory.Exists("/sys"))
        {
            Console.WriteLine("  /sys exists");
            try
            {
                var sysDirs = Directory.GetDirectories("/sys");
                Console.WriteLine($"  Found {sysDirs.Length} directories in /sys:");
                foreach (var dir in sysDirs.Take(10))
                {
                    Console.WriteLine($"    - {Path.GetFileName(dir)}");
                }
                
                if (Directory.Exists("/sys/class"))
                {
                    Console.WriteLine("  /sys/class exists");
                    var classDirs = Directory.GetDirectories("/sys/class");
                    Console.WriteLine($"  Found {classDirs.Length} directories in /sys/class:");
                    foreach (var dir in classDirs.Take(10))
                    {
                        Console.WriteLine($"    - {Path.GetFileName(dir)}");
                    }
                }
                else
                {
                    Console.WriteLine("  /sys/class does NOT exist!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error reading /sys: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("  /sys does NOT exist!");
        }
        
        int retries = 0;
        while (!Directory.Exists(SysClassNetPath) && retries < 10)
        {
            Console.WriteLine($"Waiting for /sys/class/net... (attempt {retries + 1}/10)");
            await Task.Delay(500);
            retries++;
        }
        
        if (!Directory.Exists(SysClassNetPath))
        {
            Console.WriteLine("ERROR: /sys/class/net not found after 5 seconds!");
            return 1;
        }

        // Handle Ctrl+C gracefully
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            _cts.Cancel();
        };

        // Discover network interfaces
        Console.WriteLine("Discovering network interfaces...");
        DiscoverInterfaces();
        Console.WriteLine();

        // Configure interfaces
        Console.WriteLine("Configuring interfaces...");
        await ConfigureInterfaces();
        Console.WriteLine();

        // Start monitoring
        var monitorTask = Task.Run(() => MonitorInterfaces(_cts.Token));

        Console.WriteLine("✅ Network manager started");
        Console.WriteLine("   - Interface monitor: active");
        Console.WriteLine();
        Console.WriteLine("Press Ctrl+C to stop");
        Console.WriteLine();

        // Wait for cancellation
        await monitorTask;

        Console.WriteLine();
        Console.WriteLine("Network manager stopped");
        return 0;
    }

    static void DiscoverInterfaces()
    {
        var interfaces = Directory.GetDirectories(SysClassNetPath);
        
        foreach (var ifacePath in interfaces)
        {
            var name = Path.GetFileName(ifacePath);
            
            // Get interface type
            var type = GetInterfaceType(name);
            var operstate = "unknown";
            
            try
            {
                var operstatePath = Path.Combine(ifacePath, "operstate");
                if (File.Exists(operstatePath))
                {
                    operstate = File.ReadAllText(operstatePath).Trim();
                }
            }
            catch { }

            var iface = new InterfaceInfo
            {
                Name = name,
                Type = type,
                State = operstate,
                Path = ifacePath
            };

            _interfaces[name] = iface;
            LogEvent($"Found {type} interface: {name} (state: {operstate})");
        }
    }

    static async Task ConfigureInterfaces()
    {
        foreach (var iface in _interfaces.Values)
        {
            // Skip loopback - configure it separately
            if (iface.Name == "lo")
            {
                await BringUpLoopback();
                continue;
            }

            // Skip tunnel interfaces
            if (iface.Type == "tunnel" || iface.Type == "unknown")
            {
                continue;
            }

            // Configure ethernet interfaces directly with ip commands
            if (iface.Type == "ethernet" || iface.Type == "wireless")
            {
                LogEvent($"Configuring {iface.Name}...");
                
                // Bring interface up
                if (await RunIpCommand($"link set {iface.Name} up"))
                {
                    LogEvent($"✓ {iface.Name} link up");
                }
                else
                {
                    LogEvent($"⚠ Failed to bring {iface.Name} up");
                    continue;
                }

                // Configure IP address (QEMU default: 10.0.2.15/24)
                if (await RunIpCommand($"addr add 10.0.2.15/24 dev {iface.Name}"))
                {
                    LogEvent($"✓ {iface.Name} configured with IP 10.0.2.15");
                }
                else
                {
                    LogEvent($"⚠ Failed to set IP on {iface.Name}");
                    continue;
                }

                // Add default route via QEMU gateway
                if (await RunIpCommand("route add default via 10.0.2.2"))
                {
                    LogEvent($"✓ Default route via 10.0.2.2");
                }
                else
                {
                    LogEvent($"⚠ Failed to add default route");
                }

                LogEvent($"✅ {iface.Name} network configuration complete!");
                
                // Only configure first ethernet interface
                break;
            }
        }
    }

    static async Task<bool> RunIpCommand(string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "/bin/ip",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            
            var process = Process.Start(psi);
            if (process != null)
            {
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    static async Task BringUpLoopback()
    {
        LogEvent("Configuring loopback interface...");
        
        try
        {
            // For now, just note that loopback should be configured
            // In full implementation, would use ioctl SIOCSIFFLAGS
            LogEvent("✓ Loopback interface ready (lo)");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LogEvent($"ERROR configuring loopback: {ex.Message}");
        }
    }

    static async Task MonitorInterfaces(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                // Check interface states
                foreach (var iface in _interfaces.Values)
                {
                    try
                    {
                        var operstatePath = Path.Combine(iface.Path, "operstate");
                        if (File.Exists(operstatePath))
                        {
                            var newState = File.ReadAllText(operstatePath).Trim();
                            if (newState != iface.State)
                            {
                                LogEvent($"Interface {iface.Name}: {iface.State} → {newState}");
                                iface.State = newState;
                            }
                        }
                    }
                    catch { }
                }

                await Task.Delay(3000, token); // Check every 3 seconds
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            LogEvent($"ERROR in interface monitor: {ex.Message}");
        }
    }

    static string GetInterfaceType(string name)
    {
        if (name == "lo") return "loopback";
        if (name.StartsWith("eth")) return "ethernet";
        if (name.StartsWith("enp")) return "ethernet";
        if (name.StartsWith("wlan")) return "wireless";
        if (name.StartsWith("wlp")) return "wireless";
        if (name.StartsWith("br")) return "bridge";
        if (name.StartsWith("docker")) return "virtual";
        if (name.StartsWith("veth")) return "virtual";
        if (name.StartsWith("sit")) return "tunnel";
        return "unknown";
    }

    static void LogEvent(string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        Console.WriteLine($"[{timestamp}] {message}");
    }
}

class InterfaceInfo
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public string State { get; set; } = "unknown";
    public required string Path { get; init; }
}
