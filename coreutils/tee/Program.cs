using System;
using System.IO;

class Program
{
    static int Main(string[] args)
    {
        bool append = false;
        int fileIdx = 0;
        if (args.Length > 0 && args[0] == "-a") { append = true; fileIdx = 1; }
        string file = args.Length > fileIdx ? args[fileIdx] : null;
        StreamWriter writer = null;
        if (file != null) writer = new StreamWriter(file, append);
        string line;
        while ((line = Console.ReadLine()) != null)
        {
            Console.WriteLine(line);
            if (writer != null) writer.WriteLine(line);
        }
        writer?.Close();
        return 0;
    }
}
