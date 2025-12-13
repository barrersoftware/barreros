using System;

class Program
{
    static int Main(string[] args)
    {
        string output = args.Length > 0 ? string.Join(" ", args) : "y";
        while (true) Console.WriteLine(output);
    }
}
