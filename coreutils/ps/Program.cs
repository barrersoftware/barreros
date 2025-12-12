using System;
using System.IO;
using System.Diagnostics;

// BarrerOS ps - List processes

try
{
    Console.WriteLine("  PID  CMD");
    
    // Read from /proc
    var procDirs = Directory.GetDirectories("/proc");
    
    foreach (var dir in procDirs)
    {
        var dirName = Path.GetFileName(dir);
        
        // Check if directory name is a number (PID)
        if (int.TryParse(dirName, out int pid))
        {
            try
            {
                // Read command name from /proc/[pid]/comm
                var commFile = Path.Combine(dir, "comm");
                if (File.Exists(commFile))
                {
                    var comm = File.ReadAllText(commFile).Trim();
                    Console.WriteLine($"{pid,5}  {comm}");
                }
            }
            catch
            {
                // Process might have exited, skip it
            }
        }
    }
    
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"ps: error: {ex.Message}");
    return 1;
}
