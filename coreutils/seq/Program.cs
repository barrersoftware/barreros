using System;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0) return 1;
        int start = 1, end = 1, step = 1;
        if (args.Length == 1) { end = int.Parse(args[0]); }
        else if (args.Length == 2) { start = int.Parse(args[0]); end = int.Parse(args[1]); }
        else if (args.Length >= 3) { start = int.Parse(args[0]); step = int.Parse(args[1]); end = int.Parse(args[2]); }
        for (int i = start; step > 0 ? i <= end : i >= end; i += step)
            Console.WriteLine(i);
        return 0;
    }
}
