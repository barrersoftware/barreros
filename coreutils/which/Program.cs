using System;
using System.IO;

// BarrerOS which - Locate a command

class Program
{
    static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                return 1;
            }
            
            string command = args[0];
            var paths = Environment.GetEnvironmentVariable("PATH")?.Split(':') 
                ?? new[] { "/bin", "/sbin", "/usr/bin", "/usr/sbin" };
            
            foreach (var path in paths)
            {
                var fullPath = Path.Combine(path, command);
                if (File.Exists(fullPath))
                {
                    Console.WriteLine(fullPath);
                    return 0;
                }
            }
            
            return 1;
        }
        catch
        {
            return 1;
        }
    }
}
