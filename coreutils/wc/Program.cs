using System;
using System.IO;
using System.Linq;

// BarrerOS wc - Word, line, character count

if (args.Length == 0)
{
    Console.WriteLine("wc: missing file operand");
    return 1;
}

try
{
    var file = args[0];
    var content = File.ReadAllText(file);
    var lines = content.Split('\n').Length;
    var words = content.Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    var chars = content.Length;
    
    Console.WriteLine($"  {lines} {words} {chars} {file}");
    return 0;
}
catch (FileNotFoundException)
{
    Console.WriteLine($"wc: {args[0]}: No such file");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"wc: error: {ex.Message}");
    return 1;
}
