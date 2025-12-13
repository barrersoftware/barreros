using System;
using System.IO;

// BarrerOS rmdir - Remove empty directories

if (args.Length == 0)
{
    Console.WriteLine("rmdir: missing operand");
    return 1;
}

try
{
    foreach (var dir in args)
    {
        Directory.Delete(dir, false); // false = only if empty
    }
    return 0;
}
catch (DirectoryNotFoundException)
{
    Console.WriteLine($"rmdir: failed to remove '{args[0]}': No such directory");
    return 1;
}
catch (IOException)
{
    Console.WriteLine($"rmdir: failed to remove '{args[0]}': Directory not empty");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"rmdir: error: {ex.Message}");
    return 1;
}
