using System;
using System.IO;

// BarrerOS tail - Output last lines of files

var numLines = 10; // default
var fileIndex = 0;

// Parse -n flag
if (args.Length > 0 && args[0] == "-n" && args.Length > 1)
{
    if (int.TryParse(args[1], out int n))
    {
        numLines = n;
        fileIndex = 2;
    }
}

if (args.Length == fileIndex)
{
    Console.WriteLine("tail: missing file operand");
    return 1;
}

try
{
    var file = args[fileIndex];
    var lines = File.ReadAllLines(file);
    var startIndex = Math.Max(0, lines.Length - numLines);
    
    for (int i = startIndex; i < lines.Length; i++)
    {
        Console.WriteLine(lines[i]);
    }
    
    return 0;
}
catch (FileNotFoundException)
{
    Console.WriteLine($"tail: cannot open '{args[fileIndex]}': No such file");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"tail: error: {ex.Message}");
    return 1;
}
