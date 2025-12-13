using System;
using System.IO;
using System.Text;

// BarrerOS tr - Translate or delete characters

class Program
{
    static int Main(string[] args)
    {
        try
        {
            if (args.Length < 1)
            {
                Console.WriteLine("tr: missing operand");
                Console.WriteLine("Usage: tr SET1 [SET2]");
                return 1;
            }
            
            bool delete = args[0] == "-d";
            string set1 = delete ? (args.Length > 1 ? args[1] : null) : args[0];
            string set2 = !delete && args.Length > 1 ? args[1] : null;
            
            if (set1 == null)
            {
                Console.WriteLine("tr: missing SET1");
                return 1;
            }
            
            // Read stdin character by character
            int c;
            while ((c = Console.Read()) != -1)
            {
                char ch = (char)c;
                
                if (delete)
                {
                    // Delete characters in set1
                    if (!set1.Contains(ch))
                        Console.Write(ch);
                }
                else if (set2 != null)
                {
                    // Translate set1 to set2
                    int index = set1.IndexOf(ch);
                    if (index >= 0 && index < set2.Length)
                        Console.Write(set2[index]);
                    else
                        Console.Write(ch);
                }
                else
                {
                    Console.Write(ch);
                }
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"tr: error: {ex.Message}");
            return 1;
        }
    }
}
