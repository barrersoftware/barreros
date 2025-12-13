using System;
using System.Runtime.InteropServices;

// BarrerOS uname - Print system information

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
struct UtsName
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65)]
    public string sysname;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65)]
    public string nodename;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65)]
    public string release;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65)]
    public string version;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65)]
    public string machine;
}

class Program
{
    [DllImport("libc", SetLastError = true)]
    private static extern int uname(ref UtsName buf);
    
    static int Main(string[] args)
    {
        try
        {
            var uts = new UtsName();
            if (uname(ref uts) != 0)
            {
                Console.WriteLine("uname: failed to get system information");
                return 1;
            }
            
            bool all = false;
            bool kernel = false;
            bool node = false;
            bool release = false;
            bool machine = false;
            bool version = false;
            
            // Parse flags
            if (args.Length == 0)
            {
                // Default: just kernel name
                Console.WriteLine(uts.sysname);
                return 0;
            }
            
            foreach (var arg in args)
            {
                if (arg == "-a" || arg == "--all")
                    all = true;
                else if (arg == "-s" || arg == "--kernel-name")
                    kernel = true;
                else if (arg == "-n" || arg == "--nodename")
                    node = true;
                else if (arg == "-r" || arg == "--kernel-release")
                    release = true;
                else if (arg == "-m" || arg == "--machine")
                    machine = true;
                else if (arg == "-v" || arg == "--kernel-version")
                    version = true;
            }
            
            if (all)
            {
                Console.WriteLine($"{uts.sysname} {uts.nodename} {uts.release} {uts.version} {uts.machine}");
            }
            else
            {
                var output = "";
                if (kernel) output += uts.sysname + " ";
                if (node) output += uts.nodename + " ";
                if (release) output += uts.release + " ";
                if (version) output += uts.version + " ";
                if (machine) output += uts.machine + " ";
                Console.WriteLine(output.TrimEnd());
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"uname: error: {ex.Message}");
            return 1;
        }
    }
}
