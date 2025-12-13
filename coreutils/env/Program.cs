using System;

// BarrerOS env - Print environment variables

foreach (var entry in Environment.GetEnvironmentVariables())
{
    var kvp = (System.Collections.DictionaryEntry)entry;
    Console.WriteLine($"{kvp.Key}={kvp.Value}");
}

return 0;
