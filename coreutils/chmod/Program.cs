using System;
using System.IO;
using System.Runtime.InteropServices;

// BarrerOS chmod - Change file permissions

class Program
{
    [DllImport("libc", SetLastError = true)]
    private static extern int chmod(string path, int mode);
    
    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("chmod: missing operand");
            Console.WriteLine("Usage: chmod MODE FILE");
            return 1;
        }

        try
        {
            var modeStr = args[0];
            var file = args[1];
            
            // Parse octal mode (e.g., 755, 644)
            int mode = Convert.ToInt32(modeStr, 8);
            
            var result = chmod(file, mode);
            if (result != 0)
            {
                Console.WriteLine($"chmod: cannot access '{file}'");
                return 1;
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"chmod: error: {ex.Message}");
            return 1;
        }
    }
}

