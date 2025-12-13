using System;
using System.IO;

// BarrerOS ls - Directory listing in C#

var longFormat = false;
var pathIndex = 0;

// Parse flags
if (args.Length > 0 && args[0].StartsWith('-'))
{
    if (args[0].Contains('l'))
        longFormat = true;
    pathIndex = 1;
}

var path = args.Length > pathIndex ? args[pathIndex] : Directory.GetCurrentDirectory();

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
        
        if (longFormat)
        {
            var info = new FileInfo(entry);
            var perms = isDir ? "drwxr-xr-x" : "-rw-r--r--";
            var size = isDir ? "4096" : info.Length.ToString();
            var date = info.LastWriteTime.ToString("MMM dd HH:mm");
            Console.WriteLine($"{perms} 1 root root {size,8} {date} {name}{(isDir ? "/" : "")}");
        }
        else
        {
            if (isDir)
            {
                Console.WriteLine(name + "/");
            }
            else
            {
                Console.WriteLine(name);
            }
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
