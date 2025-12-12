using System;
using System.IO;
using System.Text.RegularExpressions;

// BarrerOS grep - Search text patterns

if (args.Length < 1)
{
    Console.WriteLine("grep: missing pattern");
    Console.WriteLine("Usage: grep PATTERN [FILE...]");
    return 1;
}

var pattern = args[0];
var files = args.Length > 1 ? args[1..] : new[] { "-" };
var found = false;

try
{
    var regex = new Regex(pattern);
    
    foreach (var file in files)
    {
        string[] lines;
        
        if (file == "-")
        {
            // Read from stdin (not implemented fully yet)
            Console.WriteLine("grep: stdin not yet supported");
            continue;
        }
        
        if (!File.Exists(file))
        {
            Console.WriteLine($"grep: {file}: No such file or directory");
            continue;
        }
        
        lines = File.ReadAllLines(file);
        
        foreach (var line in lines)
        {
            if (regex.IsMatch(line))
            {
                if (files.Length > 1 && file != "-")
                {
                    Console.Write($"{file}:");
                }
                Console.WriteLine(line);
                found = true;
            }
        }
    }
    
    return found ? 0 : 1;
}
catch (Exception ex)
{
    Console.WriteLine($"grep: error: {ex.Message}");
    return 2;
}
