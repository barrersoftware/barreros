using System;
using System.IO;

// BarrerOS mv - Move/rename files

if (args.Length < 2)
{
    Console.WriteLine("mv: missing file operand");
    Console.WriteLine("Usage: mv SOURCE DEST");
    return 1;
}

try
{
    var source = args[0];
    var dest = args[1];
    
    File.Move(source, dest);
    return 0;
}
catch (FileNotFoundException)
{
    Console.WriteLine($"mv: cannot stat '{args[0]}': No such file or directory");
    return 1;
}
catch (IOException ex)
{
    Console.WriteLine($"mv: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"mv: error: {ex.Message}");
    return 1;
}
