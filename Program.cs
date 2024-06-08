using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace text_editor
{
    // TODO:
    // - Fix cursor position when entering new line in the middle of the text
    // - Add more vim bindings
    // - Add more text editing features
    // - Add syntax highlighting
    // - Add line numbers
    // - Add search and replace

    static class Editor
    {
        // Check if file exists and open it
        static int FileExist(string path)
        {
            if (File.Exists(path))
            {
                return 0;
            }
            else
            {
                Console.WriteLine("File does not exist.");
                return 1;
            }
        }

        // load file
        static string[] LoadFile(string path)
        {
            string text = File.ReadAllText(path);

            // transform text into array of lines
            string[] lines = text.Split('\n');

            return lines;
        }

        static bool IsWord(string word)
        {
            foreach (char c in word)
            {
                if (!char.IsLetter(c))
                {
                    return false;
                }
            }

            return true;
        }

        static string[] SplitWords(string line)
        {
            List<string> words = new List<string>();
            string word = "";
            foreach (char c in line)
            {
                if (char.IsLetter(c))
                {
                    word += c;
                }
                else
                {
                    if (word != "")
                    {
                        words.Add(word);
                        word = "";
                    }

                    words.Add(c.ToString()); // Add non-letter character as separate word
                }
            }

            if (word != "")
            {
                words.Add(word);
            }

            return words.ToArray();
        }

        // main function
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: text_editor <filename>");
                return;
            }

            string path = args[0];
            if (FileExist(path) == 0)
            {
                string[] lines = LoadFile(path);

                Console.Clear();
                foreach (var line in lines)
                {
                    Console.WriteLine(line);
                }

                int x = 0;
                int y = 0;

                Console.SetCursorPosition(x, y);

                string[] dictionary = SpellChecker.LoadDictionary("dictionary.txt");
                SpellChecker spellChecker = new SpellChecker(dictionary);
                while (true)
                {
                    var w = Console.WindowWidth;
                    var h = Console.WindowHeight;

                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    Console.Clear();

                    bool check_Spelling = false;
                    switch (keyInfo.Key)
                    {
                        // if backspace is pressed remove character on i == y and x position in lines[i]
                        case ConsoleKey.Backspace:
                        {
                            if (x > 0)
                            {
                                x--;
                                lines[y] = lines[y].Remove(x, 1);
                            }
                            else if (y > 0)
                            {
                                x = lines[y - 1].Length;
                                lines[y - 1] += lines[y];
                                for (var i = y; i < lines.Length - 1; i++)
                                {
                                    lines[i] = lines[i + 1];
                                }

                                Array.Resize(ref lines, lines.Length - 1);
                                y--;
                            }

                            break;
                        }
                        // elif delete is pressed remove character on i == y and x position in lines[i]
                        case ConsoleKey.Delete:
                        {
                            if (x < lines[y].Length)
                            {
                                lines[y] = lines[y].Remove(x, 1);
                            }
                            else if (y < lines.Length - 1)
                            {
                                lines[y] += lines[y + 1];
                                for (var i = y + 1; i < lines.Length - 1; i++)
                                {
                                    lines[i] = lines[i + 1];
                                }

                                Array.Resize(ref lines, lines.Length - 1);
                            }

                            break;
                        }
                        case ConsoleKey.Enter:
                        {
                            Array.Resize(ref lines, lines.Length + 1);
                            for (var i = lines.Length - 1; i > y; i--)
                            {
                                lines[i] = lines[i - 1];
                            }

                            lines[y + 1] = lines[y].Substring(x);
                            lines[y] = lines[y].Substring(0, x);
                            y++;
                            x = 0;
                            break;
                        }
                        case ConsoleKey.UpArrow:
                        {
                            if (y > 0)
                            {
                                y--;
                                x = Math.Min(x, lines[y].Length);
                            }

                            break;
                        }
                        case ConsoleKey.DownArrow:
                        {
                            if (y < lines.Length - 1)
                            {
                                y++;
                                x = Math.Min(x, lines[y].Length);
                            }

                            break;
                        }
                        case ConsoleKey.LeftArrow:
                        {
                            if (x > 0)
                            {
                                x--;
                            }
                            else if (y > 0)
                            {
                                y--;
                                x = lines[y].Length;
                            }

                            break;
                        }
                        case ConsoleKey.RightArrow:
                        {
                            if (x < lines[y].Length)
                            {
                                x++;
                            }
                            else if (y < lines.Length - 1)
                            {
                                y++;
                                x = 0;
                            }

                            break;
                        }
                        default:
                        {
                            if (x < w - 1)
                            {
                                lines[y] = lines[y].Insert(x, keyInfo.KeyChar.ToString());
                                x++;
                            }

                            check_Spelling = true;
                            break;
                        }
                    }

                    // Correctly parse the words in the file, ignore special characters
                    // Check if the word is spelled correctly
                    // Print the word with red color if it is not spelled correctly

                    for (var i = 0; i < lines.Length; i++)
                    {
                        // split line into words by space
                        string[] words = SplitWords(lines[i]);

                        foreach (var word in words)
                        {
                            if (check_Spelling && IsWord(word))
                            {
                                // Remove special characters and Lowercase the word
                                string wordToCheck = new string(word.Where(char.IsLetter).ToArray());
                                wordToCheck = wordToCheck.ToLower();

                                if (!spellChecker.Check(wordToCheck.Trim()))
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                }

                                Console.Write(word);
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.Write(word);
                            }
                        }

                        Console.WriteLine();
                    }

                    if (keyInfo.Modifiers == ConsoleModifiers.Control && keyInfo.Key == ConsoleKey.S)
                    {
                        File.WriteAllText(path, string.Join("\n", lines));
                        Console.SetCursorPosition(0, h - 1);
                        Console.Write($"File: {path} | Line: {(1 + y)} | Column: {(x + 1)} | Saved!");
                    } // save file and exit
                    else if (keyInfo.Modifiers == ConsoleModifiers.Control && keyInfo.Key == ConsoleKey.W)
                    {
                        File.WriteAllText(path, string.Join("\n", lines));
                        break;
                    } // exit without saving
                    else if (keyInfo.Modifiers == ConsoleModifiers.Control && keyInfo.Key == ConsoleKey.C)
                    {
                        break;
                    }

                    Console.SetCursorPosition(0, h - 1);
                    Console.Write($"File: {path} | Line: {(1 + y)} | Column: {(x + 1)}");

                    Console.SetCursorPosition(x, y);
                }

                Console.Clear();
            }
        }
    }
}
