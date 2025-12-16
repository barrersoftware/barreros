using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: nslookup <domain> [dns-server]");
            Console.WriteLine("Example: nslookup google.com");
            Console.WriteLine("         nslookup google.com 8.8.8.8");
            return 1;
        }

        string domain = args[0];
        string dnsServer = args.Length > 1 ? args[1] : "8.8.8.8"; // Default to Google DNS

        try
        {
            Console.WriteLine($"Querying {domain} via {dnsServer}...");
            
            var addresses = QueryDNS(domain, dnsServer);
            
            if (addresses.Length == 0)
            {
                Console.WriteLine("No addresses found");
                return 1;
            }

            Console.WriteLine($"\nName:    {domain}");
            Console.WriteLine("Addresses:");
            foreach (var addr in addresses)
            {
                Console.WriteLine($"  {addr}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    static string[] QueryDNS(string domain, string dnsServer)
    {
        // Build DNS query packet
        byte[] query = BuildDNSQuery(domain);

        // Send UDP query to DNS server
        using (var udpClient = new UdpClient())
        {
            udpClient.Connect(dnsServer, 53);
            udpClient.Send(query, query.Length);
            
            // Wait for response (5 second timeout)
            udpClient.Client.ReceiveTimeout = 5000;
            
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] response = udpClient.Receive(ref remoteEndPoint);
            
            // Parse DNS response
            return ParseDNSResponse(response);
        }
    }

    static byte[] BuildDNSQuery(string domain)
    {
        // DNS query structure:
        // Header (12 bytes) + Question section
        
        // Transaction ID (random 16-bit)
        Random rnd = new Random();
        ushort transactionId = (ushort)rnd.Next(0, 65535);
        
        // Build query
        var query = new System.Collections.Generic.List<byte>();
        
        // Header section (12 bytes)
        query.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)transactionId)));
        query.AddRange(new byte[] { 0x01, 0x00 }); // Flags: standard query
        query.AddRange(new byte[] { 0x00, 0x01 }); // Questions: 1
        query.AddRange(new byte[] { 0x00, 0x00 }); // Answer RRs: 0
        query.AddRange(new byte[] { 0x00, 0x00 }); // Authority RRs: 0
        query.AddRange(new byte[] { 0x00, 0x00 }); // Additional RRs: 0
        
        // Question section
        // Domain name (encoded as length-prefixed labels)
        foreach (string label in domain.Split('.'))
        {
            query.Add((byte)label.Length);
            query.AddRange(Encoding.ASCII.GetBytes(label));
        }
        query.Add(0x00); // Null terminator
        
        query.AddRange(new byte[] { 0x00, 0x01 }); // Type: A (IPv4 address)
        query.AddRange(new byte[] { 0x00, 0x01 }); // Class: IN (Internet)
        
        return query.ToArray();
    }

    static string[] ParseDNSResponse(byte[] response)
    {
        var addresses = new System.Collections.Generic.List<string>();
        
        if (response.Length < 12)
            return addresses.ToArray();
        
        // Skip header (12 bytes)
        int pos = 12;
        
        // Skip question section (we know what we asked)
        // Find the null terminator
        while (pos < response.Length && response[pos] != 0)
        {
            if ((response[pos] & 0xC0) == 0xC0) // Compression pointer
            {
                pos += 2;
                break;
            }
            pos += response[pos] + 1;
        }
        if (pos < response.Length && response[pos] == 0)
            pos++; // Skip null terminator
        pos += 4; // Skip QTYPE and QCLASS
        
        // Parse answer section
        ushort answerCount = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(response, 6));
        
        for (int i = 0; i < answerCount && pos < response.Length; i++)
        {
            // Skip name (usually compressed)
            if ((response[pos] & 0xC0) == 0xC0)
            {
                pos += 2; // Compression pointer
            }
            else
            {
                while (pos < response.Length && response[pos] != 0)
                    pos += response[pos] + 1;
                pos++; // Skip null terminator
            }
            
            if (pos + 10 > response.Length)
                break;
            
            ushort type = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(response, pos));
            pos += 2;
            
            ushort cls = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(response, pos));
            pos += 2;
            
            uint ttl = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(response, pos));
            pos += 4;
            
            ushort dataLen = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(response, pos));
            pos += 2;
            
            // If this is an A record (IPv4 address)
            if (type == 1 && dataLen == 4 && pos + 4 <= response.Length)
            {
                var ip = new IPAddress(new byte[] { 
                    response[pos], 
                    response[pos + 1], 
                    response[pos + 2], 
                    response[pos + 3] 
                });
                addresses.Add(ip.ToString());
            }
            
            pos += dataLen;
        }
        
        return addresses.ToArray();
    }
}
