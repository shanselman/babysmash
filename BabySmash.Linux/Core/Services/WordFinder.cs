using System.IO;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Text;
using Avalonia.Controls;

namespace BabySmash.Linux.Core.Services;

// Interface so WordFinder doesn't need direct reference to CoolLetter
public interface ILetterFigure
{
    char Character { get; }
}

public class WordFinder
{
    private const int MinimumWordLength = 2, MaximumWordLength = 15;

    private volatile bool _wordsReady;
    private readonly HashSet<string> _words = new();

    public WordFinder()
    {
        LoadWordsAsync();
    }

    private void LoadWordsAsync()
    {
        Thread t = new(() =>
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream("BabySmash.Linux.Core.Resources.Words.txt");
                if (stream != null)
                {
                    using var sr = new StreamReader(stream);
                    string? s = sr.ReadLine();
                    while (s != null)
                    {
                        // Ignore invalid lines, comment lines, or words which are too short or too long.
                        if (!s.Contains(";") && !s.Contains("/") && !s.Contains("\\") &&
                            s.Length >= MinimumWordLength && s.Length <= MaximumWordLength)
                        {
                            _words.Add(s.ToUpper());
                        }
                        s = sr.ReadLine();
                    }
                }
                _wordsReady = true;
            }
            catch
            {
                // Silently fail - words feature just won't be available
            }
        })
        {
            IsBackground = true
        };
        t.Start();
    }

    /// <summary>
    /// Checks the figures queue for the last word typed (matching Windows behavior)
    /// </summary>
    public string? LastWord(IList<Control> figuresQueue)
    {
        // If not done loading, or could not yet form a word based on queue length, just abort.
        int figuresPos = figuresQueue.Count - 1;
        if (!_wordsReady || figuresPos < MinimumWordLength - 1)
        {
            return null;
        }

        // Loop while the most recently pressed things are still letters
        string? longestWord = null;
        var stringToCheck = new StringBuilder();
        int lowestIndexToCheck = Math.Max(0, figuresPos - MaximumWordLength);
        
        while (figuresPos >= lowestIndexToCheck)
        {
            if (figuresQueue[figuresPos] is ILetterFigure letterFigure)
            {
                // Build up the string and check to see if it is a word so far.
                stringToCheck.Insert(0, char.ToUpper(letterFigure.Character));
                string s = stringToCheck.ToString();
                if (_words.Contains(s) && s.Length >= MinimumWordLength)
                {
                    longestWord = s;
                }
                figuresPos--;
            }
            else
            {
                // If we encounter a non-letter, stop
                break;
            }
        }

        return longestWord;
    }
}
