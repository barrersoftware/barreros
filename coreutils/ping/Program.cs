using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

// BarrerOS ping command - ICMP echo test

if (args.Length == 0)
{
    Console.WriteLine("Usage: ping <hostname>");
    return 1;
}

var target = args[0];
var count = 4;

Console.WriteLine($"PING {target}");

using var ping = new Ping();
var successful = 0;
var failed = 0;
var totalTime = 0L;

for (int i = 0; i < count; i++)
{
    try
    {
        var reply = await ping.SendPingAsync(target, 5000);
        
        if (reply.Status == IPStatus.Success)
        {
            successful++;
            totalTime += reply.RoundtripTime;
            Console.WriteLine($"Reply from {reply.Address}: bytes=32 time={reply.RoundtripTime}ms TTL={reply.Options?.Ttl ?? 0}");
        }
        else
        {
            failed++;
            Console.WriteLine($"Request timed out (status: {reply.Status})");
        }
    }
    catch (Exception ex)
    {
        failed++;
        Console.WriteLine($"Ping failed: {ex.Message}");
    }
    
    if (i < count - 1)
        await Task.Delay(1000);
}

Console.WriteLine();
Console.WriteLine($"--- {target} ping statistics ---");
Console.WriteLine($"{count} packets transmitted, {successful} received, {failed} lost");

if (successful > 0)
{
    var avgTime = totalTime / successful;
    Console.WriteLine($"rtt avg = {avgTime}ms");
}

return successful > 0 ? 0 : 1;
