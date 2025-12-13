using System;
using System.IO;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0) { Console.WriteLine("basename: missing operand"); return 1; }
        string path = args[0];
        string suffix = args.Length > 1 ? args[1] : null;
        string basename = Path.GetFileName(path);
        if (suffix != null && basename.EndsWith(suffix)) basename = basename.Substring(0, basename.Length - suffix.Length);
        Console.WriteLine(basename);
        return 0;
    }
}
