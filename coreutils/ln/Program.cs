using System;
using System.IO;

// BarrerOS ln - Create symbolic links

if (args.Length < 2)
{
    Console.WriteLine("ln: missing operand");
    Console.WriteLine("Usage: ln -s TARGET LINK_NAME");
    return 1;
}

try
{
    if (args[0] != "-s")
    {
        Console.WriteLine("ln: only symbolic links (-s) supported");
        return 1;
    }
    
    var target = args[1];
    var linkName = args[2];
    
    File.CreateSymbolicLink(linkName, target);
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"ln: error: {ex.Message}");
    return 1;
}
