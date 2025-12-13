using System;
using System.IO;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0) { Console.WriteLine("dirname: missing operand"); return 1; }
        string dirname = Path.GetDirectoryName(args[0]);
        if (string.IsNullOrEmpty(dirname)) dirname = ".";
        Console.WriteLine(dirname);
        return 0;
    }
}
