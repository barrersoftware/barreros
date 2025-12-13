using System;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            foreach (var entry in Environment.GetEnvironmentVariables())
            {
                var de = (System.Collections.DictionaryEntry)entry;
                Console.WriteLine($"{de.Key}={de.Value}");
            }
        }
        else
        {
            var val = Environment.GetEnvironmentVariable(args[0]);
            if (val != null) Console.WriteLine(val);
        }
        return 0;
    }
}
