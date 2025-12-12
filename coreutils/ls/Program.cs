using System;
using System.IO;

// BarrerOS ls - Directory listing in C#
// Simple proof of concept

var path = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

try
{
    var entries = Directory.GetFileSystemEntries(path);
    
    if (entries.Length == 0)
    {
        // Empty directory
        return 0;
    }
    
    foreach (var entry in entries)
    {
        var name = Path.GetFileName(entry);
        var isDir = Directory.Exists(entry);
        
        if (isDir)
        {
            Console.WriteLine(name + "/");
        }
        else
        {
            Console.WriteLine(name);
        }
    }
    
    return 0;
}
catch (DirectoryNotFoundException)
{
    Console.WriteLine($"ls: cannot access '{path}': No such file or directory");
    return 1;
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine($"ls: cannot access '{path}': Permission denied");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"ls: error: {ex.Message}");
    return 1;
}
