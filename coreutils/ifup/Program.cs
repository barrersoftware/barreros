using System;
using System.Runtime.InteropServices;
using System.Net;
using System.Text;

// BarrerOS ifup - Bring up network interface with IP configuration
// Simple wrapper around system commands for now

if (args.Length == 0)
{
    Console.WriteLine("BarrerOS ifup - Configure network interface");
    Console.WriteLine("Usage: ifup <interface> [ip_address]");
    Console.WriteLine("Example: ifup eth0 10.0.2.15");
    return 1;
}

var ifaceName = args[0];
var ipAddress = args.Length > 1 ? args[1] : "10.0.2.15"; // QEMU default

Console.WriteLine($"Configuring {ifaceName} with IP {ipAddress}...");

// For now, create a simple shell script to configure the interface
// In a full implementation, we'd use ioctl directly
var script = $@"#!/bin/sh
ip link set {ifaceName} up
ip addr add {ipAddress}/24 dev {ifaceName}
ip route add default via 10.0.2.2
";

try
{
    System.IO.File.WriteAllText("/tmp/ifup.sh", script);
    
    var psi = new System.Diagnostics.ProcessStartInfo
    {
        FileName = "/bin/sh",
        Arguments = "/tmp/ifup.sh",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };
    
    var proc = System.Diagnostics.Process.Start(psi);
    if (proc != null)
    {
        proc.WaitForExit();
        if (proc.ExitCode == 0)
        {
            Console.WriteLine($"✓ {ifaceName} configured with IP {ipAddress}");
            Console.WriteLine($"✓ Default route via 10.0.2.2");
            return 0;
        }
    }
    
    Console.WriteLine("ERROR: Failed to configure interface");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
    return 1;
}
