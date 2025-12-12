using System;
using System.IO;

// BarrerOS cat - Concatenate and print files

if (args.Length == 0)
{
    Console.WriteLine("cat: missing file operand");
    return 1;
}

try
{
    foreach (var file in args)
    {
        var content = File.ReadAllText(file);
        Console.Write(content);
    }
    return 0;
}
catch (FileNotFoundException)
{
    Console.WriteLine($"cat: {args[0]}: No such file or directory");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"cat: error: {ex.Message}");
    return 1;
}
