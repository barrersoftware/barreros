using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

// BarrerOS tar - Basic tape archive utility
// Simple implementation: create and extract tar archives

class Program
{
    static int Main(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: tar -c[f] archive.tar [files...] (create)");
                Console.WriteLine("       tar -x[f] archive.tar (extract)");
                Console.WriteLine("       tar -t[f] archive.tar (list)");
                return 1;
            }

            bool create = args[0].Contains('c');
            bool extract = args[0].Contains('x');
            bool list = args[0].Contains('t');
            
            string archivePath = args[1];
            
            if (create)
            {
                // Create archive (simple: use zip format, compatible)
                var files = args.Skip(2).ToArray();
                if (files.Length == 0)
                {
                    Console.WriteLine("tar: no files specified");
                    return 1;
                }
                
                using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
                {
                    foreach (var file in files)
                    {
                        if (File.Exists(file))
                        {
                            archive.CreateEntryFromFile(file, Path.GetFileName(file));
                            Console.WriteLine(file);
                        }
                    }
                }
                
                return 0;
            }
            else if (extract)
            {
                if (!File.Exists(archivePath))
                {
                    Console.WriteLine($"tar: {archivePath}: Cannot open: No such file");
                    return 1;
                }
                
                ZipFile.ExtractToDirectory(archivePath, ".");
                return 0;
            }
            else if (list)
            {
                if (!File.Exists(archivePath))
                {
                    Console.WriteLine($"tar: {archivePath}: Cannot open: No such file");
                    return 1;
                }
                
                using (var archive = ZipFile.OpenRead(archivePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        Console.WriteLine(entry.FullName);
                    }
                }
                
                return 0;
            }
            
            Console.WriteLine("tar: must specify -c, -x, or -t");
            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"tar: error: {ex.Message}");
            return 1;
        }
    }
}
