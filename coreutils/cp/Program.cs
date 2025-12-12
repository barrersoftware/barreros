using System;
using System.IO;

// BarrerOS cp - Copy files

if (args.Length < 2)
{
    Console.WriteLine("cp: missing file operand");
    Console.WriteLine("Usage: cp SOURCE DEST");
    return 1;
}

try
{
    var source = args[0];
    var dest = args[1];
    
    File.Copy(source, dest, overwrite: false);
    return 0;
}
catch (FileNotFoundException)
{
    Console.WriteLine($"cp: cannot stat '{args[0]}': No such file or directory");
    return 1;
}
catch (IOException ex)
{
    Console.WriteLine($"cp: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"cp: error: {ex.Message}");
    return 1;
}
