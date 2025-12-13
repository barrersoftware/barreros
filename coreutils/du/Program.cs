using System;
using System.IO;

// BarrerOS du - Estimate file space usage

class Program
{
    static long GetDirectorySize(string path, bool summarize)
    {
        try
        {
            long size = 0;
            
            // Add files in current directory
            foreach (var file in Directory.GetFiles(path))
            {
                try
                {
                    var fi = new FileInfo(file);
                    size += fi.Length;
                }
                catch { }
            }
            
            // Recurse into subdirectories
            foreach (var dir in Directory.GetDirectories(path))
            {
                try
                {
                    long dirSize = GetDirectorySize(dir, summarize);
                    size += dirSize;
                    
                    if (!summarize)
                    {
                        Console.WriteLine($"{dirSize / 1024}\t{dir}");
                    }
                }
                catch { }
            }
            
            return size;
        }
        catch
        {
            return 0;
        }
    }
    
    static int Main(string[] args)
    {
        try
        {
            bool summarize = false;
            int pathIndex = 0;
            
            // Parse flags
            if (args.Length > 0 && args[0].StartsWith('-'))
            {
                if (args[0].Contains('s'))
                    summarize = true;
                pathIndex = 1;
            }
            
            var path = args.Length > pathIndex ? args[pathIndex] : ".";
            
            if (!Directory.Exists(path))
            {
                Console.WriteLine($"du: cannot access '{path}': No such file or directory");
                return 1;
            }
            
            long totalSize = GetDirectorySize(path, summarize);
            Console.WriteLine($"{totalSize / 1024}\t{path}");
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"du: error: {ex.Message}");
            return 1;
        }
    }
}
