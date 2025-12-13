using System;
using System.Runtime.InteropServices;

// BarrerOS killp - Terminate processes
// Usage: killp PID [PID...]

class Program
{
    [DllImport("libc", SetLastError = true)]
    private static extern int kill(int pid, int signal);
    
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("killp: missing operand");
            Console.WriteLine("Usage: killp PID [PID...]");
            return 1;
        }

        try
        {
            const int SIGTERM = 15;
            
            foreach (var arg in args)
            {
                if (!int.TryParse(arg, out int pid))
                {
                    Console.WriteLine($"killp: invalid PID: {arg}");
                    continue;
                }
                
                // Send SIGTERM (graceful termination)
                var result = kill(pid, SIGTERM);
                
                if (result != 0)
                {
                    Console.WriteLine($"killp: cannot terminate process {pid}");
                }
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"killp: error: {ex.Message}");
            return 1;
        }
    }
}

