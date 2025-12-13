using System;
using System.IO;

// BarrerOS free - Display amount of free and used memory

class Program
{
    static int Main(string[] args)
    {
        try
        {
            if (!File.Exists("/proc/meminfo"))
            {
                Console.WriteLine("free: cannot read /proc/meminfo");
                return 1;
            }
            
            long memTotal = 0;
            long memFree = 0;
            long memAvailable = 0;
            long buffers = 0;
            long cached = 0;
            long swapTotal = 0;
            long swapFree = 0;
            
            foreach (var line in File.ReadAllLines("/proc/meminfo"))
            {
                var parts = line.Split(new[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;
                
                if (parts[0] == "MemTotal" && long.TryParse(parts[1], out long val))
                    memTotal = val;
                else if (parts[0] == "MemFree" && long.TryParse(parts[1], out val))
                    memFree = val;
                else if (parts[0] == "MemAvailable" && long.TryParse(parts[1], out val))
                    memAvailable = val;
                else if (parts[0] == "Buffers" && long.TryParse(parts[1], out val))
                    buffers = val;
                else if (parts[0] == "Cached" && long.TryParse(parts[1], out val))
                    cached = val;
                else if (parts[0] == "SwapTotal" && long.TryParse(parts[1], out val))
                    swapTotal = val;
                else if (parts[0] == "SwapFree" && long.TryParse(parts[1], out val))
                    swapFree = val;
            }
            
            long memUsed = memTotal - memFree - buffers - cached;
            long swapUsed = swapTotal - swapFree;
            
            Console.WriteLine("               total        used        free      shared  buff/cache   available");
            Console.WriteLine($"Mem:        {memTotal,8}    {memUsed,8}    {memFree,8}           0    {buffers + cached,8}    {memAvailable,8}");
            Console.WriteLine($"Swap:       {swapTotal,8}    {swapUsed,8}    {swapFree,8}");
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"free: error: {ex.Message}");
            return 1;
        }
    }
}
