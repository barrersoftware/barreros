using System;
using System.IO;

// BarrerOS touch - Create empty files or update timestamps

if (args.Length == 0)
{
    Console.WriteLine("touch: missing file operand");
    return 1;
}

try
{
    foreach (var file in args)
    {
        if (File.Exists(file))
        {
            // Update timestamp
            File.SetLastWriteTimeUtc(file, DateTime.UtcNow);
        }
        else
        {
            // Create empty file
            File.Create(file).Dispose();
        }
    }
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"touch: error: {ex.Message}");
    return 1;
}
