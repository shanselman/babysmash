using System.Reflection;
using System.Text;

namespace BabySmash.Core.Services;

public class WordFinder
{
    private const int MinimumWordLength = 2, MaximumWordLength = 15;

    private bool wordsReady;
    private readonly HashSet<string> words = new();
    private readonly StringBuilder currentSequence = new();

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
                using var stream = assembly.GetManifestResourceStream("BabySmash.Core.Resources.Words.txt");
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
                            this.words.Add(s.ToUpper());
                        }
                        s = sr.ReadLine();
                    }
                }
                wordsReady = true;
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
        if (!wordsReady || !char.IsLetter(letter))
            return null;

        currentSequence.Append(char.ToUpper(letter));

        // Check progressively longer sequences
        string? longestWord = null;
        for (int len = Math.Min(currentSequence.Length, MaximumWordLength); len >= MinimumWordLength; len--)
        {
            if (currentSequence.Length >= len)
            {
                string candidate = currentSequence.ToString(currentSequence.Length - len, len);
                if (words.Contains(candidate))
                {
                    longestWord = candidate;
                    break; // Found the longest match
                }
            }
        }

        // Keep sequence manageable
        if (currentSequence.Length > MaximumWordLength)
        {
            currentSequence.Remove(0, currentSequence.Length - MaximumWordLength);
        }

        return longestWord;
    }

    public void Reset()
    {
        currentSequence.Clear();
    }
}
