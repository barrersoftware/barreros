using System;
using System.IO;
using System.Runtime.InteropServices;

// BarrerOS df - Report file system disk space usage

[StructLayout(LayoutKind.Sequential)]
struct StatVFS
{
    public ulong f_bsize;    // Filesystem block size
    public ulong f_frsize;   // Fragment size
    public ulong f_blocks;   // Size of fs in f_frsize units
    public ulong f_bfree;    // Free blocks
    public ulong f_bavail;   // Free blocks for unprivileged users
    public ulong f_files;    // Number of inodes
    public ulong f_ffree;    // Free inodes
    public ulong f_favail;   // Free inodes for unprivileged users
    public ulong f_fsid;     // Filesystem ID
    public ulong f_flag;     // Mount flags
    public ulong f_namemax;  // Maximum filename length
}

class Program
{
    [DllImport("libc", SetLastError = true)]
    private static extern int statvfs(string path, ref StatVFS buf);
    
    static int Main(string[] args)
    {
        try
        {
            // Header
            Console.WriteLine("Filesystem           1K-blocks      Used Available Use% Mounted on");
            
            // Get mounted filesystems
            var mountPoints = new[] { "/", "/proc", "/sys", "/dev", "/tmp" };
            
            foreach (var mount in mountPoints)
            {
                if (!Directory.Exists(mount))
                    continue;
                    
                var stat = new StatVFS();
                if (statvfs(mount, ref stat) == 0)
                {
                    var blockSize = stat.f_frsize;
                    var totalBlocks = stat.f_blocks;
                    var freeBlocks = stat.f_bfree;
                    var availBlocks = stat.f_bavail;
                    
                    var totalKB = (totalBlocks * blockSize) / 1024;
                    var usedKB = ((totalBlocks - freeBlocks) * blockSize) / 1024;
                    var availKB = (availBlocks * blockSize) / 1024;
                    
                    var usePercent = totalKB > 0 ? (usedKB * 100) / totalKB : 0;
                    
                    var fsName = mount == "/" ? "/dev/sda" : mount;
                    Console.WriteLine($"{fsName,-20} {totalKB,10} {usedKB,9} {availKB,9} {usePercent,3}% {mount}");
                }
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"df: error: {ex.Message}");
            return 1;
        }
    }
}
