using System.IO;
using System.Threading;
using System.Collections.Generic;
using System;
﻿using System.Reflection;
using System.Text;

namespace BabySmash.Linux.Core.Services;

public class WordFinder
{
    private const int MinimumWordLength = 2, MaximumWordLength = 15;

    private volatile bool _wordsReady;
    private readonly HashSet<string> _words = new();
    private readonly StringBuilder _currentSequence = new();
    private readonly object _lock = new();

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
                            lock (_lock)
                            {
                                _words.Add(s.ToUpper());
                            }
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

    public string? AddLetter(char letter)
    {
        if (!_wordsReady || !char.IsLetter(letter))
            return null;

        lock (_lock)
        {
            _currentSequence.Append(char.ToUpper(letter));

            // Check progressively longer sequences
            string? longestWord = null;
            for (int len = Math.Min(_currentSequence.Length, MaximumWordLength); len >= MinimumWordLength; len--)
            {
                if (_currentSequence.Length >= len)
                {
                    string candidate = _currentSequence.ToString(_currentSequence.Length - len, len);
                    if (_words.Contains(candidate))
                    {
                        longestWord = candidate;
                        break; // Found the longest match
                    }
                }
            }

            // Keep sequence manageable
            if (_currentSequence.Length > MaximumWordLength)
            {
                _currentSequence.Remove(0, _currentSequence.Length - MaximumWordLength);
            }

            return longestWord;
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _currentSequence.Clear();
        }
    }
}
