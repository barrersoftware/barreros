using System;
using System.IO;

// BarrerOS rm - Remove files

if (args.Length == 0)
{
    Console.WriteLine("rm: missing operand");
    return 1;
}

try
{
    foreach (var file in args)
    {
        File.Delete(file);
    }
    return 0;
}
catch (FileNotFoundException)
{
    Console.WriteLine($"rm: cannot remove '{args[0]}': No such file or directory");
    return 1;
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine($"rm: cannot remove '{args[0]}': Permission denied");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"rm: error: {ex.Message}");
    return 1;
}
