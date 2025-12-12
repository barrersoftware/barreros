using System;
using System.IO;
using System.Collections.Generic;

// BarrerOS nano - Simple text editor (nano-style)
// Phase 1: Basic implementation - read, edit, save

var filename = args.Length > 0 ? args[0] : null;
var lines = new List<string>();

// Load file if exists
if (filename != null && File.Exists(filename))
{
    lines.AddRange(File.ReadAllLines(filename));
}
else if (filename == null)
{
    Console.WriteLine("Usage: nano <filename>");
    return 1;
}

// Add empty line if file is empty
if (lines.Count == 0)
{
    lines.Add("");
}

int cursorLine = 0;
int cursorCol = 0;
bool running = true;
bool modified = false;

// Simple display function (not full terminal control yet)
void Display()
{
    Console.Clear();
    Console.WriteLine($"BarrerOS nano 1.0 - Editing: {filename}" + (modified ? " [Modified]" : ""));
    Console.WriteLine(new string('-', 60));
    
    for (int i = 0; i < lines.Count; i++)
    {
        if (i == cursorLine)
        {
            Console.Write("> ");
        }
        else
        {
            Console.Write("  ");
        }
        Console.WriteLine(lines[i]);
    }
    
    Console.WriteLine(new string('-', 60));
    Console.WriteLine("Commands: (s)ave, (q)uit, (a)dd line, (d)elete line, (e)dit current line");
    Console.WriteLine($"Line {cursorLine + 1}/{lines.Count}");
}

while (running)
{
    Display();
    
    var key = Console.ReadKey(true);
    
    switch (key.KeyChar)
    {
        case 's': // Save
        case 'S':
            try
            {
                File.WriteAllLines(filename!, lines);
                modified = false;
                Console.WriteLine("\nFile saved! Press any key...");
                Console.ReadKey(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError saving: {ex.Message}");
                Console.ReadKey(true);
            }
            break;
            
        case 'q': // Quit
        case 'Q':
            if (modified)
            {
                Console.Write("\nFile modified. Save before quitting? (y/n): ");
                var answer = Console.ReadKey();
                if (answer.KeyChar == 'y' || answer.KeyChar == 'Y')
                {
                    File.WriteAllLines(filename!, lines);
                }
            }
            running = false;
            break;
            
        case 'a': // Add line
        case 'A':
            Console.Write("\nEnter new line: ");
            var newLine = Console.ReadLine() ?? "";
            lines.Insert(cursorLine + 1, newLine);
            cursorLine++;
            modified = true;
            break;
            
        case 'd': // Delete line
        case 'D':
            if (lines.Count > 1)
            {
                lines.RemoveAt(cursorLine);
                if (cursorLine >= lines.Count)
                    cursorLine = lines.Count - 1;
                modified = true;
            }
            break;
            
        case 'e': // Edit current line
        case 'E':
            Console.Write($"\nEdit line {cursorLine + 1}: ");
            var edited = Console.ReadLine();
            if (edited != null)
            {
                lines[cursorLine] = edited;
                modified = true;
            }
            break;
    }
    
    // Arrow keys for navigation
    if (key.Key == ConsoleKey.UpArrow && cursorLine > 0)
    {
        cursorLine--;
    }
    else if (key.Key == ConsoleKey.DownArrow && cursorLine < lines.Count - 1)
    {
        cursorLine++;
    }
}

Console.Clear();
Console.WriteLine("nano: File closed.");
return 0;
