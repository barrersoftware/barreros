using System;
using System.IO;

// BarrerOS head - Output first lines of files

var numLines = 10; // default
var fileIndex = 0;

// Parse -n flag or -NUM shorthand
if (args.Length > 0 && args[0].StartsWith('-'))
{
    if (args[0] == "-n" && args.Length > 1)
    {
        if (int.TryParse(args[1], out int n))
        {
            numLines = n;
            fileIndex = 2;
        }
    }
    else if (int.TryParse(args[0].Substring(1), out int num))
    {
        // -5 format
        numLines = num;
        fileIndex = 1;
    }
}

if (args.Length == fileIndex)
{
    Console.WriteLine("head: missing file operand");
    return 1;
}

try
{
    var file = args[fileIndex];
    var lines = File.ReadAllLines(file);
    var count = Math.Min(numLines, lines.Length);
    
    for (int i = 0; i < count; i++)
    {
        Console.WriteLine(lines[i]);
    }
    
    return 0;
}
catch (FileNotFoundException)
{
    Console.WriteLine($"head: cannot open '{args[fileIndex]}': No such file");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"head: error: {ex.Message}");
    return 1;
}
