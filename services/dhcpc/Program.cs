using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

// BarrerOS DHCP Client - Automatic network configuration
// Implements DHCP protocol to get IP, gateway, DNS from DHCP server

Console.WriteLine("🌐 BarrerOS DHCP Client");
Console.WriteLine();

// Get network interface (eth0)
var iface = args.Length > 0 ? args[0] : "eth0";

try
{
    // Get interface info
    var nic = NetworkInterface.GetAllNetworkInterfaces()
        .FirstOrDefault(n => n.Name == iface);
    
    if (nic == null)
    {
        Console.WriteLine($"❌ Interface {iface} not found");
        return 1;
    }
    
    var macAddress = nic.GetPhysicalAddress().GetAddressBytes();
    Console.WriteLine($"Interface: {iface}");
    Console.WriteLine($"MAC: {BitConverter.ToString(macAddress).Replace("-", ":")}");
    Console.WriteLine();
    
    // Create UDP socket for DHCP
    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    socket.EnableBroadcast = true;
    socket.Bind(new IPEndPoint(IPAddress.Any, 68)); // DHCP client port
    
    // Generate transaction ID
    var xid = (uint)Random.Shared.Next();
    
    // Send DHCP DISCOVER
    Console.WriteLine("📡 Sending DHCP DISCOVER...");
    var discover = BuildDHCPDiscover(xid, macAddress);
    socket.SendTo(discover, new IPEndPoint(IPAddress.Broadcast, 67)); // DHCP server port
    
    // Wait for DHCP OFFER
    Console.WriteLine("⏳ Waiting for DHCP OFFER...");
    socket.ReceiveTimeout = 10000; // 10 second timeout
    
    var buffer = new byte[1024];
    EndPoint serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
    var received = socket.ReceiveFrom(buffer, ref serverEndpoint);
    
    // Parse DHCP OFFER
    var offer = ParseDHCPMessage(buffer, received);
    
    if (offer.MessageType != 2) // DHCPOFFER
    {
        Console.WriteLine($"❌ Expected OFFER, got message type {offer.MessageType}");
        return 1;
    }
    
    Console.WriteLine($"✅ DHCP OFFER received");
    Console.WriteLine($"   Offered IP: {offer.YourIP}");
    Console.WriteLine($"   Server: {offer.ServerIP}");
    Console.WriteLine($"   Gateway: {offer.Gateway}");
    Console.WriteLine($"   DNS: {string.Join(", ", offer.DNSServers)}");
    Console.WriteLine();
    
    // Send DHCP REQUEST
    Console.WriteLine("📡 Sending DHCP REQUEST...");
    var request = BuildDHCPRequest(xid, macAddress, offer.YourIP, offer.ServerIP);
    socket.SendTo(request, new IPEndPoint(IPAddress.Broadcast, 67));
    
    // Wait for DHCP ACK
    Console.WriteLine("⏳ Waiting for DHCP ACK...");
    received = socket.ReceiveFrom(buffer, ref serverEndpoint);
    
    var ack = ParseDHCPMessage(buffer, received);
    
    if (ack.MessageType != 5) // DHCPACK
    {
        Console.WriteLine($"❌ Expected ACK, got message type {ack.MessageType}");
        return 1;
    }
    
    Console.WriteLine($"✅ DHCP ACK received");
    Console.WriteLine($"   IP: {ack.YourIP}");
    Console.WriteLine($"   Subnet: {ack.SubnetMask}");
    Console.WriteLine($"   Gateway: {ack.Gateway}");
    Console.WriteLine($"   DNS: {string.Join(", ", ack.DNSServers)}");
    Console.WriteLine($"   Lease: {ack.LeaseTime} seconds");
    Console.WriteLine();
    
    // Configure network interface
    Console.WriteLine("⚙️  Configuring network interface...");
    
    // Calculate CIDR from subnet mask
    var cidr = CalculateCIDR(ack.SubnetMask);
    
    // Bring interface up
    RunCommand("ip", $"link set {iface} up");
    Thread.Sleep(500);
    
    // Set IP address
    RunCommand("ip", $"addr add {ack.YourIP}/{cidr} dev {iface}");
    
    // Set default gateway
    if (!string.IsNullOrEmpty(ack.Gateway))
    {
        RunCommand("ip", $"route add default via {ack.Gateway}");
    }
    
    Console.WriteLine($"✅ Interface configured: {ack.YourIP}/{cidr}");
    Console.WriteLine();
    
    // Write /etc/resolv.conf
    if (ack.DNSServers.Count > 0)
    {
        Console.WriteLine("📝 Writing /etc/resolv.conf...");
        using (var resolvconf = File.CreateText("/etc/resolv.conf"))
        {
            foreach (var dns in ack.DNSServers)
            {
                resolvconf.WriteLine($"nameserver {dns}");
            }
        }
        Console.WriteLine("✅ DNS configured");
    }
    
    Console.WriteLine();
    Console.WriteLine("🎉 Network configuration complete!");
    
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"❌ DHCP failed: {ex.Message}");
    return 1;
}

static byte[] BuildDHCPDiscover(uint xid, byte[] macAddress)
{
    var packet = new byte[300];
    
    packet[0] = 1;  // BOOTREQUEST
    packet[1] = 1;  // Ethernet
    packet[2] = 6;  // Hardware address length
    packet[3] = 0;  // Hops
    
    // Transaction ID
    Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)xid)), 0, packet, 4, 4);
    
    // Client MAC address
    Array.Copy(macAddress, 0, packet, 28, 6);
    
    // Magic cookie
    packet[236] = 0x63;
    packet[237] = 0x82;
    packet[238] = 0x53;
    packet[239] = 0x63;
    
    // DHCP options
    var pos = 240;
    
    // Message type: DISCOVER
    packet[pos++] = 53;
    packet[pos++] = 1;
    packet[pos++] = 1; // DHCPDISCOVER
    
    // Requested IP parameters
    packet[pos++] = 55; // Parameter request list
    packet[pos++] = 4;
    packet[pos++] = 1;  // Subnet mask
    packet[pos++] = 3;  // Router
    packet[pos++] = 6;  // DNS
    packet[pos++] = 15; // Domain name
    
    // End option
    packet[pos++] = 255;
    
    return packet;
}

static byte[] BuildDHCPRequest(uint xid, byte[] macAddress, string requestedIP, string serverIP)
{
    var packet = new byte[300];
    
    packet[0] = 1;  // BOOTREQUEST
    packet[1] = 1;  // Ethernet
    packet[2] = 6;  // Hardware address length
    packet[3] = 0;  // Hops
    
    // Transaction ID
    Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)xid)), 0, packet, 4, 4);
    
    // Client MAC address
    Array.Copy(macAddress, 0, packet, 28, 6);
    
    // Magic cookie
    packet[236] = 0x63;
    packet[237] = 0x82;
    packet[238] = 0x53;
    packet[239] = 0x63;
    
    // DHCP options
    var pos = 240;
    
    // Message type: REQUEST
    packet[pos++] = 53;
    packet[pos++] = 1;
    packet[pos++] = 3; // DHCPREQUEST
    
    // Requested IP address
    packet[pos++] = 50;
    packet[pos++] = 4;
    var ipBytes = IPAddress.Parse(requestedIP).GetAddressBytes();
    Array.Copy(ipBytes, 0, packet, pos, 4);
    pos += 4;
    
    // Server identifier
    packet[pos++] = 54;
    packet[pos++] = 4;
    var serverBytes = IPAddress.Parse(serverIP).GetAddressBytes();
    Array.Copy(serverBytes, 0, packet, pos, 4);
    pos += 4;
    
    // End option
    packet[pos++] = 255;
    
    return packet;
}

static DHCPMessage ParseDHCPMessage(byte[] buffer, int length)
{
    var msg = new DHCPMessage();
    
    // Your IP address
    var yourIP = new byte[4];
    Array.Copy(buffer, 16, yourIP, 0, 4);
    msg.YourIP = new IPAddress(yourIP).ToString();
    
    // Server IP address
    var serverIP = new byte[4];
    Array.Copy(buffer, 20, serverIP, 0, 4);
    msg.ServerIP = new IPAddress(serverIP).ToString();
    
    // Parse options (start after magic cookie at offset 240)
    var pos = 240;
    while (pos < length && buffer[pos] != 255) // End option
    {
        var optionCode = buffer[pos++];
        if (optionCode == 0) continue; // Padding
        
        var optionLen = buffer[pos++];
        var optionData = new byte[optionLen];
        Array.Copy(buffer, pos, optionData, 0, optionLen);
        pos += optionLen;
        
        switch (optionCode)
        {
            case 53: // Message type
                msg.MessageType = optionData[0];
                break;
            case 1: // Subnet mask
                msg.SubnetMask = new IPAddress(optionData).ToString();
                break;
            case 3: // Router (gateway)
                msg.Gateway = new IPAddress(optionData).ToString();
                break;
            case 6: // DNS servers
                for (int i = 0; i < optionLen; i += 4)
                {
                    var dnsBytes = new byte[4];
                    Array.Copy(optionData, i, dnsBytes, 0, 4);
                    msg.DNSServers.Add(new IPAddress(dnsBytes).ToString());
                }
                break;
            case 51: // Lease time
                msg.LeaseTime = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(optionData, 0));
                break;
        }
    }
    
    return msg;
}

static int CalculateCIDR(string subnetMask)
{
    var bytes = IPAddress.Parse(subnetMask).GetAddressBytes();
    var bits = 0;
    foreach (var b in bytes)
    {
        for (int i = 7; i >= 0; i--)
        {
            if ((b & (1 << i)) != 0)
                bits++;
            else
                return bits;
        }
    }
    return bits;
}

static void RunCommand(string command, string args)
{
    var process = Process.Start(new ProcessStartInfo
    {
        FileName = command,
        Arguments = args,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    });
    
    if (process != null)
    {
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
                Console.WriteLine($"⚠️  {error.Trim()}");
        }
    }
}

class DHCPMessage
{
    public byte MessageType { get; set; }
    public string YourIP { get; set; } = "";
    public string ServerIP { get; set; } = "";
    public string SubnetMask { get; set; } = "";
    public string Gateway { get; set; } = "";
    public List<string> DNSServers { get; set; } = new();
    public int LeaseTime { get; set; }
}
