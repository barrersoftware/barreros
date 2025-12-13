using System;
using System.Threading;

// BarrerOS sleep - Delay for specified time

if (args.Length == 0)
{
    Console.WriteLine("sleep: missing operand");
    return 1;
}

try
{
    var seconds = int.Parse(args[0]);
    Thread.Sleep(seconds * 1000);
    return 0;
}
catch (FormatException)
{
    Console.WriteLine($"sleep: invalid time interval '{args[0]}'");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"sleep: error: {ex.Message}");
    return 1;
}
