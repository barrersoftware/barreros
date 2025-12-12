using System;
using System.IO;

// BarrerOS mkdir - Create directories

if (args.Length == 0)
{
    Console.WriteLine("mkdir: missing operand");
    return 1;
}

try
{
    foreach (var dir in args)
    {
        Directory.CreateDirectory(dir);
    }
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"mkdir: error: {ex.Message}");
    return 1;
}
