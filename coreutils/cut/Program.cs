using System;
using System.IO;

// BarrerOS cut - Remove sections from each line of files

class Program
{
    static int Main(string[] args)
    {
        try
        {
            string delimiter = "\t";
            string fields = null;
            int fieldIndex = 0;
            
            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-d" && i + 1 < args.Length)
                {
                    delimiter = args[i + 1];
                    i++;
                }
                else if (args[i] == "-f" && i + 1 < args.Length)
                {
                    fields = args[i + 1];
                    i++;
                }
                else if (!args[i].StartsWith('-'))
                {
                    fieldIndex = i;
                    break;
                }
            }
            
            if (fields == null)
            {
                Console.WriteLine("cut: you must specify a list of fields");
                Console.WriteLine("Usage: cut -f FIELD [-d DELIM] [file]");
                return 1;
            }
            
            string file = fieldIndex > 0 && fieldIndex < args.Length ? args[fieldIndex] : null;
            
            // Parse field number (simple: just first field for now)
            if (!int.TryParse(fields, out int fieldNum))
            {
                Console.WriteLine("cut: invalid field specification");
                return 1;
            }
            
            // Read input
            var lines = file != null ? File.ReadAllLines(file) : ReadStdin();
            
            // Process each line
            foreach (var line in lines)
            {
                var parts = line.Split(new[] { delimiter }, StringSplitOptions.None);
                if (fieldNum > 0 && fieldNum <= parts.Length)
                {
                    Console.WriteLine(parts[fieldNum - 1]);
                }
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"cut: error: {ex.Message}");
            return 1;
        }
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
