using System;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Diagnostics;

// BarrerOS 'ip' command - Network configuration tool
// Simple implementation for basic interface management

if (args.Length == 0)
{
    ShowHelp();
    return 1;
}

var command = args[0];

switch (command)
{
    case "link":
        if (args.Length >= 2 && args[1] == "show")
        {
            ShowInterfaces();
            return 0;
        }
        else if (args.Length >= 4 && args[1] == "set")
        {
            var iface = args[2];
            var state = args[3]; // "up" or "down"
            return SetInterfaceState(iface, state);
        }
        break;
        
    case "addr":
        if (args.Length >= 2 && args[1] == "show")
        {
            ShowAddresses();
            return 0;
        }
        else if (args.Length >= 5 && args[1] == "add")
        {
            // ip addr add 127.0.0.1/8 dev lo
            var addr = args[2];
            var dev = args[4];
            return AddAddress(dev, addr);
        }
        break;
}

Console.WriteLine($"Unknown command: {string.Join(" ", args)}");
ShowHelp();
return 1;

static void ShowHelp()
{
    Console.WriteLine("BarrerOS ip - Network configuration");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  ip link show              - Show network interfaces");
    Console.WriteLine("  ip link set <if> up       - Bring interface up");
    Console.WriteLine("  ip link set <if> down     - Bring interface down");
    Console.WriteLine("  ip addr show              - Show IP addresses");
    Console.WriteLine("  ip addr add <ip> dev <if> - Add IP address");
}

static void ShowInterfaces()
{
    var interfaces = NetworkInterface.GetAllNetworkInterfaces();
    foreach (var iface in interfaces)
    {
        var state = iface.OperationalStatus == OperationalStatus.Up ? "UP" : "DOWN";
        var type = iface.NetworkInterfaceType;
        
        Console.WriteLine($"{iface.Name}: <{state}> mtu {iface.GetIPProperties().GetIPv4Properties()?.Mtu ?? 1500}");
        Console.WriteLine($"    link/{type.ToString().ToLower()} {iface.GetPhysicalAddress()}");
    }
}

static void ShowAddresses()
{
    var interfaces = NetworkInterface.GetAllNetworkInterfaces();
    foreach (var iface in interfaces)
    {
        var props = iface.GetIPProperties();
        Console.WriteLine($"{iface.Name}:");
        
        foreach (var addr in props.UnicastAddresses)
        {
            Console.WriteLine($"    inet {addr.Address}/{addr.PrefixLength}");
        }
    }
}

static int SetInterfaceState(string ifaceName, string state)
{
    // Use ProcessStartInfo to call ifconfig as fallback
    // In a real implementation, we'd use ioctl via P/Invoke
    try
    {
        var action = state == "up" ? "up" : "down";
        var psi = new ProcessStartInfo
        {
            FileName = "/sbin/ifconfig",
            Arguments = $"{ifaceName} {action}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        
        var proc = Process.Start(psi);
        if (proc == null)
        {
            Console.WriteLine($"Failed to execute ifconfig");
            return 1;
        }
        
        proc.WaitForExit();
        return proc.ExitCode;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine($"Note: Interface state changes require root/CAP_NET_ADMIN");
        return 1;
    }
}

static int AddAddress(string ifaceName, string address)
{
    try
    {
        var psi = new ProcessStartInfo
        {
            FileName = "/sbin/ifconfig",
            Arguments = $"{ifaceName} {address.Split('/')[0]}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        
        var proc = Process.Start(psi);
        if (proc == null)
        {
            Console.WriteLine($"Failed to execute ifconfig");
            return 1;
        }
        
        proc.WaitForExit();
        return proc.ExitCode;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine($"Note: Adding addresses requires root/CAP_NET_ADMIN");
        return 1;
    }
}
