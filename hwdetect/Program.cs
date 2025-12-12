using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

// BarrerOS Hardware Detection
// Detects CPU, GPU, storage, network devices from /sys and /proc

Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("🔍 BarrerOS Hardware Detection v1.0");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();

// Detect CPU
Console.WriteLine("CPU Information:");
if (File.Exists("/proc/cpuinfo"))
{
    var cpuinfo = File.ReadAllLines("/proc/cpuinfo");
    var modelLine = cpuinfo.FirstOrDefault(l => l.StartsWith("model name"));
    var coresLine = cpuinfo.Where(l => l.StartsWith("processor")).Count();
    
    if (modelLine != null)
    {
        var model = modelLine.Split(':')[1].Trim();
        Console.WriteLine($"  Model: {model}");
    }
    Console.WriteLine($"  Cores: {coresLine}");
}
Console.WriteLine();

// Detect Memory
Console.WriteLine("Memory Information:");
if (File.Exists("/proc/meminfo"))
{
    var meminfo = File.ReadAllLines("/proc/meminfo");
    var totalLine = meminfo.FirstOrDefault(l => l.StartsWith("MemTotal"));
    
    if (totalLine != null)
    {
        var totalKB = long.Parse(totalLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
        var totalMB = totalKB / 1024;
        var totalGB = totalMB / 1024.0;
        Console.WriteLine($"  Total: {totalGB:F2} GB ({totalMB} MB)");
    }
}
Console.WriteLine();

// Detect PCI Devices (GPU, Network, etc.)
Console.WriteLine("PCI Devices:");
if (Directory.Exists("/sys/bus/pci/devices"))
{
    var devices = Directory.GetDirectories("/sys/bus/pci/devices");
    var gpuCount = 0;
    var netCount = 0;
    var storageCount = 0;
    
    foreach (var device in devices)
    {
        try
        {
            var classFile = Path.Combine(device, "class");
            if (!File.Exists(classFile)) continue;
            
            var classCode = File.ReadAllText(classFile).Trim();
            
            // GPU: 0x03xxxx (Display controller)
            if (classCode.StartsWith("0x03"))
            {
                var vendor = GetPCIVendor(device);
                var deviceName = GetPCIDevice(device);
                Console.WriteLine($"  GPU {gpuCount++}: {vendor} {deviceName}");
            }
            // Network: 0x02xxxx (Network controller)
            else if (classCode.StartsWith("0x02"))
            {
                var vendor = GetPCIVendor(device);
                var deviceName = GetPCIDevice(device);
                Console.WriteLine($"  Network {netCount++}: {vendor} {deviceName}");
            }
            // Storage: 0x01xxxx (Mass storage controller)
            else if (classCode.StartsWith("0x01"))
            {
                var vendor = GetPCIVendor(device);
                var deviceName = GetPCIDevice(device);
                Console.WriteLine($"  Storage {storageCount++}: {vendor} {deviceName}");
            }
        }
        catch { /* Skip devices we can't read */ }
    }
}
Console.WriteLine();

// Detect Block Devices (disks)
Console.WriteLine("Storage Devices:");
if (Directory.Exists("/sys/block"))
{
    var blocks = Directory.GetDirectories("/sys/block");
    foreach (var block in blocks)
    {
        var name = Path.GetFileName(block);
        // Skip loop and ram devices
        if (name.StartsWith("loop") || name.StartsWith("ram")) continue;
        
        try
        {
            var sizePath = Path.Combine(block, "size");
            if (File.Exists(sizePath))
            {
                var sectors = long.Parse(File.ReadAllText(sizePath).Trim());
                var sizeGB = (sectors * 512) / 1024.0 / 1024.0 / 1024.0;
                Console.WriteLine($"  /dev/{name}: {sizeGB:F2} GB");
            }
        }
        catch { }
    }
}
Console.WriteLine();

Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("Detection complete!");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

return 0;

// Helper functions
static string GetPCIVendor(string devicePath)
{
    try
    {
        var vendorFile = Path.Combine(devicePath, "vendor");
        if (File.Exists(vendorFile))
        {
            var vendorId = File.ReadAllText(vendorFile).Trim();
            // Map common vendor IDs
            return vendorId switch
            {
                "0x8086" => "Intel",
                "0x10de" => "NVIDIA",
                "0x1002" => "AMD",
                "0x14e4" => "Broadcom",
                _ => vendorId
            };
        }
    }
    catch { }
    return "Unknown";
}

static string GetPCIDevice(string devicePath)
{
    try
    {
        var deviceFile = Path.Combine(devicePath, "device");
        if (File.Exists(deviceFile))
        {
            return File.ReadAllText(deviceFile).Trim();
        }
    }
    catch { }
    return "Unknown";
}
