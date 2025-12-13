using System;
using System.IO;
using System.Linq;

// BarrerOS sort - Sort lines of text files

class Program
{
    static int Main(string[] args)
    {
        try
        {
            bool reverse = false;
            bool numeric = false;
            bool unique = false;
            int fileIndex = 0;
            
            // Parse flags
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-r")
                    reverse = true;
                else if (args[i] == "-n")
                    numeric = true;
                else if (args[i] == "-u")
                    unique = true;
                else if (!args[i].StartsWith('-'))
                {
                    fileIndex = i;
                    break;
                }
            }
            
            string file = fileIndex > 0 ? args[fileIndex] : null;
            
            // Read input
            var lines = file != null ? File.ReadAllLines(file) : ReadStdin();
            
            // Sort
            if (numeric)
            {
                var sorted = lines
                    .Select(line => new { Line = line, Num = ParseNumber(line) })
                    .OrderBy(x => x.Num)
                    .Select(x => x.Line);
                
                lines = (reverse ? sorted.Reverse() : sorted).ToArray();
            }
            else
            {
                lines = reverse ? lines.OrderByDescending(x => x).ToArray() : lines.OrderBy(x => x).ToArray();
            }
            
            // Remove duplicates if -u
            if (unique)
            {
                lines = lines.Distinct().ToArray();
            }
            
            // Output
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"sort: error: {ex.Message}");
            return 1;
        }
    }
    
    static double ParseNumber(string line)
    {
        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0 && double.TryParse(parts[0], out double num))
            return num;
        return 0;
    }
    
    static string[] ReadStdin()
    {
        var lines = new System.Collections.Generic.List<string>();
        string line;
        while ((line = Console.ReadLine()) != null)
        {
            lines.Add(line);
        }
        return lines.ToArray();
    }
}
