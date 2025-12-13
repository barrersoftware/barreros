using System;
using System.IO;
using System.Text.RegularExpressions;

// BarrerOS sed - Stream editor for filtering and transforming text
// Basic implementation: s/pattern/replacement/ syntax

class Program
{
    static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                Console.WriteLine("sed: missing operand");
                Console.WriteLine("Usage: sed 's/pattern/replacement/' [file]");
                return 1;
            }
            
            string command = args[0];
            string file = args.Length > 1 ? args[1] : null;
            
            // Parse s/pattern/replacement/ command
            if (!command.StartsWith("s/"))
            {
                Console.WriteLine("sed: only 's/pattern/replacement/' syntax supported");
                return 1;
            }
            
            var parts = command.Substring(2).Split('/');
            if (parts.Length < 2)
            {
                Console.WriteLine("sed: invalid substitution syntax");
                return 1;
            }
            
            string pattern = parts[0];
            string replacement = parts[1];
            bool globalReplace = parts.Length > 2 && parts[2].Contains('g');
            
            // Read input
            var lines = file != null ? File.ReadAllLines(file) : ReadStdin();
            
            // Process each line
            foreach (var line in lines)
            {
                if (globalReplace)
                {
                    Console.WriteLine(line.Replace(pattern, replacement));
                }
                else
                {
                    // Replace first occurrence only
                    int index = line.IndexOf(pattern);
                    if (index >= 0)
                    {
                        Console.WriteLine(line.Substring(0, index) + replacement + 
                                        line.Substring(index + pattern.Length));
                    }
                    else
                    {
                        Console.WriteLine(line);
                    }
                }
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"sed: error: {ex.Message}");
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
