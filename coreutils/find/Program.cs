using System;
using System.IO;

// BarrerOS find - Search for files in a directory hierarchy

class Program
{
    static void FindFiles(string path, string namePattern, int maxDepth, int currentDepth = 0)
    {
        try
        {
            if (maxDepth > 0 && currentDepth >= maxDepth)
                return;
            
            // Print current directory if it matches
            if (string.IsNullOrEmpty(namePattern) || Path.GetFileName(path).Contains(namePattern))
            {
                Console.WriteLine(path);
            }
            
            // Search files in current directory
            try
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    if (string.IsNullOrEmpty(namePattern) || Path.GetFileName(file).Contains(namePattern))
                    {
                        Console.WriteLine(file);
                    }
                }
            }
            catch { }
            
            // Recurse into subdirectories
            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    FindFiles(dir, namePattern, maxDepth, currentDepth + 1);
                }
            }
            catch { }
        }
        catch { }
    }
    
    static int Main(string[] args)
    {
        try
        {
            string startPath = ".";
            string namePattern = null;
            int maxDepth = -1;
            
            // Simple argument parsing
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-name" && i + 1 < args.Length)
                {
                    namePattern = args[i + 1];
                    i++;
                }
                else if (args[i] == "-maxdepth" && i + 1 < args.Length)
                {
                    if (int.TryParse(args[i + 1], out int depth))
                        maxDepth = depth;
                    i++;
                }
                else if (!args[i].StartsWith('-'))
                {
                    startPath = args[i];
                }
            }
            
            if (!Directory.Exists(startPath) && !File.Exists(startPath))
            {
                Console.WriteLine($"find: '{startPath}': No such file or directory");
                return 1;
            }
            
            if (File.Exists(startPath))
            {
                Console.WriteLine(startPath);
                return 0;
            }
            
            FindFiles(startPath, namePattern, maxDepth);
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"find: error: {ex.Message}");
            return 1;
        }
    }
}
