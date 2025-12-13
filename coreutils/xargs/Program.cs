using System;
using System.Diagnostics;
using System.Collections.Generic;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0) { Console.WriteLine("xargs: missing command"); return 1; }
        string cmd = args[0];
        var cmdArgs = new List<string>();
        for (int i = 1; i < args.Length; i++) cmdArgs.Add(args[i]);
        string line;
        while ((line = Console.ReadLine()) != null)
        {
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            cmdArgs.AddRange(parts);
        }
        var proc = Process.Start(cmd, string.Join(" ", cmdArgs));
        proc?.WaitForExit();
        return proc?.ExitCode ?? 1;
    }
}
