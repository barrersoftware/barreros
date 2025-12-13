using System;
using System.IO;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0) return 1;
        if (args.Length == 2 && args[0] == "-f") return File.Exists(args[1]) ? 0 : 1;
        if (args.Length == 2 && args[0] == "-d") return Directory.Exists(args[1]) ? 0 : 1;
        if (args.Length == 2 && args[0] == "-e") return (File.Exists(args[1]) || Directory.Exists(args[1])) ? 0 : 1;
        if (args.Length == 3 && args[1] == "=") return args[0] == args[2] ? 0 : 1;
        return 1;
    }
}
