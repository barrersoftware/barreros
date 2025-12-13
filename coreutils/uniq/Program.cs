using System;
using System.IO;

// BarrerOS uniq - Report or omit repeated lines

class Program
{
    static int Main(string[] args)
    {
        try
        {
            bool count = false;
            int fileIndex = 0;
            
            // Parse flags
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-c")
                    count = true;
                else if (!args[i].StartsWith('-'))
                {
                    fileIndex = i;
                    break;
                }
            }
            
            string file = fileIndex > 0 ? args[fileIndex] : null;
            
            // Read input
            var lines = file != null ? File.ReadAllLines(file) : ReadStdin();
            
            // Process lines
            string prevLine = null;
            int lineCount = 0;
            
            foreach (var line in lines)
            {
                if (line == prevLine)
                {
                    lineCount++;
                }
                else
                {
                    if (prevLine != null)
                    {
                        if (count)
                            Console.WriteLine($"{lineCount,7} {prevLine}");
                        else
                            Console.WriteLine(prevLine);
                    }
                    prevLine = line;
                    lineCount = 1;
                }
            }
            
            // Print last line
            if (prevLine != null)
            {
                if (count)
                    Console.WriteLine($"{lineCount,7} {prevLine}");
                else
                    Console.WriteLine(prevLine);
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"uniq: error: {ex.Message}");
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
