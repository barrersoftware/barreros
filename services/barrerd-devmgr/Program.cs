using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

// BarrerOS Device Manager (barrerd-devmgr)
// Monitors /sys for device changes, manages /dev nodes, loads drivers

namespace BarrerOS.Services.DeviceManager;

class DeviceManager
{
    private const string SysPath = "/sys";
    private const string DevPath = "/dev";
    private const string SysBlockPath = "/sys/block";
    private const string SysClassPath = "/sys/class";
    private const string SysBusPciPath = "/sys/bus/pci/devices";
    
    private static readonly ConcurrentDictionary<string, DeviceInfo> _devices = new();
    private static readonly CancellationTokenSource _cts = new();
    private static int _deviceCount = 0;

    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("🔌 BarrerOS Device Manager v1.0");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine();

        // Verify /sys is mounted
        if (!Directory.Exists(SysPath))
        {
            Console.WriteLine("ERROR: /sys not mounted!");
            return 1;
        }

        // Verify /dev exists
        if (!Directory.Exists(DevPath))
        {
            Console.WriteLine("ERROR: /dev does not exist!");
            return 1;
        }

        Console.WriteLine($"System paths:");
        Console.WriteLine($"  /sys: ✓");
        Console.WriteLine($"  /dev: ✓");
        Console.WriteLine();

        // Handle Ctrl+C gracefully
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            _cts.Cancel();
        };

        // Initial device scan
        Console.WriteLine("Performing initial device scan...");
        ScanAllDevices();
        Console.WriteLine($"Found {_deviceCount} devices");
        Console.WriteLine();

        // Start monitoring tasks
        var blockMonitor = Task.Run(() => MonitorBlockDevices(_cts.Token));
        var pciMonitor = Task.Run(() => MonitorPciDevices(_cts.Token));
        
        Console.WriteLine("✅ Device manager started");
        Console.WriteLine("   - Block device monitor: active");
        Console.WriteLine("   - PCI device monitor: active");
        Console.WriteLine();
        Console.WriteLine("Press Ctrl+C to stop");
        Console.WriteLine();

        // Wait for cancellation
        await Task.WhenAll(blockMonitor, pciMonitor);

        Console.WriteLine();
        Console.WriteLine("Device manager stopped");
        return 0;
    }

    static void ScanAllDevices()
    {
        // Scan block devices (disks, partitions)
        if (Directory.Exists(SysBlockPath))
        {
            var blocks = Directory.GetDirectories(SysBlockPath);
            foreach (var block in blocks)
            {
                var name = Path.GetFileName(block);
                if (name.StartsWith("loop") || name.StartsWith("ram")) continue;
                
                RegisterDevice(new DeviceInfo
                {
                    Name = name,
                    Type = "block",
                    Path = block,
                    DevNode = $"/dev/{name}"
                });
            }
        }

        // Scan PCI devices
        if (Directory.Exists(SysBusPciPath))
        {
            var devices = Directory.GetDirectories(SysBusPciPath);
            foreach (var device in devices)
            {
                try
                {
                    var name = Path.GetFileName(device);
                    var classFile = Path.Combine(device, "class");
                    
                    if (!File.Exists(classFile)) continue;
                    
                    var classCode = File.ReadAllText(classFile).Trim();
                    var deviceType = GetDeviceType(classCode);
                    
                    if (deviceType != "unknown")
                    {
                        RegisterDevice(new DeviceInfo
                        {
                            Name = name,
                            Type = deviceType,
                            Path = device,
                            DevNode = $"pci:{name}"
                        });
                    }
                }
                catch { }
            }
        }

        // Scan network devices
        var netPath = Path.Combine(SysClassPath, "net");
        if (Directory.Exists(netPath))
        {
            var netDevices = Directory.GetDirectories(netPath);
            foreach (var netDev in netDevices)
            {
                var name = Path.GetFileName(netDev);
                if (name == "lo") continue; // Skip loopback
                
                RegisterDevice(new DeviceInfo
                {
                    Name = name,
                    Type = "network",
                    Path = netDev,
                    DevNode = $"net:{name}"
                });
            }
        }

        // Scan input devices
        var inputPath = Path.Combine(SysClassPath, "input");
        if (Directory.Exists(inputPath))
        {
            var inputDevices = Directory.GetDirectories(inputPath);
            foreach (var inputDev in inputDevices)
            {
                var name = Path.GetFileName(inputDev);
                if (!name.StartsWith("event")) continue;
                
                RegisterDevice(new DeviceInfo
                {
                    Name = name,
                    Type = "input",
                    Path = inputDev,
                    DevNode = $"/dev/input/{name}"
                });
            }
        }
    }

    static async Task MonitorBlockDevices(CancellationToken token)
    {
        try
        {
            var knownDevices = new HashSet<string>();
            
            while (!token.IsCancellationRequested)
            {
                if (Directory.Exists(SysBlockPath))
                {
                    var currentDevices = Directory.GetDirectories(SysBlockPath)
                        .Select(Path.GetFileName)
                        .Where(n => !n!.StartsWith("loop") && !n!.StartsWith("ram"))
                        .ToHashSet();

                    // Check for new devices
                    var newDevices = currentDevices.Except(knownDevices!).ToList();
                    foreach (var device in newDevices)
                    {
                        LogEvent($"New block device: {device}");
                        RegisterDevice(new DeviceInfo
                        {
                            Name = device!,
                            Type = "block",
                            Path = Path.Combine(SysBlockPath, device!),
                            DevNode = $"/dev/{device}"
                        });
                    }

                    // Check for removed devices
                    var removedDevices = knownDevices.Except(currentDevices!).ToList();
                    foreach (var device in removedDevices)
                    {
                        LogEvent($"Removed block device: {device}");
                        UnregisterDevice(device!);
                    }

                    knownDevices = currentDevices!;
                }

                await Task.Delay(2000, token); // Check every 2 seconds
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            LogEvent($"ERROR in block device monitor: {ex.Message}");
        }
    }

    static async Task MonitorPciDevices(CancellationToken token)
    {
        try
        {
            // PCI devices rarely change after boot, but we monitor anyway
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(5000, token); // Check every 5 seconds
                // In a real implementation, we'd check for hot-plug events
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            LogEvent($"ERROR in PCI device monitor: {ex.Message}");
        }
    }

    static void RegisterDevice(DeviceInfo device)
    {
        if (_devices.TryAdd(device.Name, device))
        {
            _deviceCount++;
            LogEvent($"Registered: {device.Type} device '{device.Name}' at {device.DevNode}");
        }
    }

    static void UnregisterDevice(string name)
    {
        if (_devices.TryRemove(name, out var device))
        {
            _deviceCount--;
            LogEvent($"Unregistered: {device.Type} device '{device.Name}'");
        }
    }

    static string GetDeviceType(string classCode)
    {
        return classCode switch
        {
            string c when c.StartsWith("0x01") => "storage",
            string c when c.StartsWith("0x02") => "network",
            string c when c.StartsWith("0x03") => "display",
            string c when c.StartsWith("0x04") => "multimedia",
            string c when c.StartsWith("0x0c03") => "usb",
            _ => "unknown"
        };
    }

    static void LogEvent(string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        Console.WriteLine($"[{timestamp}] {message}");
    }
}

record DeviceInfo
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required string Path { get; init; }
    public required string DevNode { get; init; }
}
