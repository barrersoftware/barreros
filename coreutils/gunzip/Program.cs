using System;
using System.IO;
using System.IO.Compression;

// BarrerOS gunzip - Decompress gzip files

class Program
{
    static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                Console.WriteLine("gunzip: missing file operand");
                Console.WriteLine("Usage: gunzip file.gz");
                return 1;
            }
            
            string inputFile = args[0];
            
            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"gunzip: {inputFile}: No such file");
                return 1;
            }
            
            if (!inputFile.EndsWith(".gz"))
            {
                Console.WriteLine($"gunzip: {inputFile}: unknown suffix -- ignored");
                return 1;
            }
            
            string outputFile = inputFile.Substring(0, inputFile.Length - 3);
            
            using (var input = File.OpenRead(inputFile))
            using (var gzip = new GZipStream(input, CompressionMode.Decompress))
            using (var output = File.Create(outputFile))
            {
                gzip.CopyTo(output);
            }
            
            // Remove compressed file (standard gunzip behavior)
            File.Delete(inputFile);
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"gunzip: error: {ex.Message}");
            return 1;
        }
    }
}
