using System;
using System.IO;
using System.Runtime.InteropServices;

// BarrerOS whoami - Print current username

try
{
    // Get UID directly on Linux
    var uid = getuid();
    
    // Try to read from /etc/passwd
    if (File.Exists("/etc/passwd"))
    {
        foreach (var line in File.ReadAllLines("/etc/passwd"))
        {
            var parts = line.Split(':');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int lineUid) && lineUid == uid)
            {
                Console.WriteLine(parts[0]);
                return 0;
            }
        }
    }
    
    // Fallback: just print UID
    Console.WriteLine($"uid{uid}");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"whoami: error: {ex.Message}");
    return 1;
}

[DllImport("libc", SetLastError = true)]
static extern uint getuid();
