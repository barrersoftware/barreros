using System;
using System.IO;
using System.Linq;

// BarrerOS diff - Compare files line by line

class Program
{
    static int Main(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: diff file1 file2");
                return 1;
            }
            
            string file1 = args[0];
            string file2 = args[1];
            
            if (!File.Exists(file1))
            {
                Console.WriteLine($"diff: {file1}: No such file");
                return 1;
            }
            
            if (!File.Exists(file2))
            {
                Console.WriteLine($"diff: {file2}: No such file");
                return 1;
            }
            
            var lines1 = File.ReadAllLines(file1);
            var lines2 = File.ReadAllLines(file2);
            
            bool hasDifference = false;
            int maxLines = Math.Max(lines1.Length, lines2.Length);
            
            for (int i = 0; i < maxLines; i++)
            {
                string line1 = i < lines1.Length ? lines1[i] : null;
                string line2 = i < lines2.Length ? lines2[i] : null;
                
                if (line1 != line2)
                {
                    if (!hasDifference)
                    {
                        Console.WriteLine($"--- {file1}");
                        Console.WriteLine($"+++ {file2}");
                        hasDifference = true;
                    }
                    
                    if (line1 != null)
                        Console.WriteLine($"- {line1}");
                    if (line2 != null)
                        Console.WriteLine($"+ {line2}");
                }
            }
            
            return hasDifference ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"diff: error: {ex.Message}");
            return 1;
        }
    }
}
