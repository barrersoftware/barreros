using System;
using System.IO;
using System.IO.Compression;

// BarrerOS gzip - Compress files using gzip

class Program
{
    static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                Console.WriteLine("gzip: missing file operand");
                Console.WriteLine("Usage: gzip file");
                return 1;
            }
            
            string inputFile = args[0];
            
            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"gzip: {inputFile}: No such file");
                return 1;
            }
            
            string outputFile = inputFile + ".gz";
            
            using (var input = File.OpenRead(inputFile))
            using (var output = File.Create(outputFile))
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                input.CopyTo(gzip);
            }
            
            // Remove original file (standard gzip behavior)
            File.Delete(inputFile);
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"gzip: error: {ex.Message}");
            return 1;
        }
    }
}
