using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BabySmash
{
    public class WordFinder
    {
        private const int MinimumWordLength = 2, MaximumWordLength = 15;

        private bool wordsReady;

        private HashSet<string> words = new HashSet<string>();

        public WordFinder(string wordsFilePath)
        {
            // File path provided should be relative to our running location, so combine for full path safety.
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            wordsFilePath = Path.Combine(dir, wordsFilePath);

            // Bail if the source word file is not found.
            if (!File.Exists(wordsFilePath))
            {
                // Source word file was not found; place a 'words.txt' file next to BabySmash.exe to enable combining 
                // letters into typed words. Some common names may work too (but successful OS speech synth may vary).
                return;
            }

            // Load up the string dictionary in the background.
            Thread t = new Thread(() =>
            {
                // Read through the word file and create a hashtable entry for each one with some 
                // further parsed word data (such as various game scores, etc)
                StreamReader sr = new StreamReader(wordsFilePath);
                string s = sr.ReadLine();
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

                // Store all words into separate buckets based on the last letter for faster compares.

                // Mark that we're done loading so we can speak words instead of just letters.
                wordsReady = true;
            });
            t.IsBackground = true;
            t.Start();
        }

        public string LastWord(List<UserControl> figuresQueue)
        {
            // If not done loading, or could not yet form a word based on queue length, just abort.
            int figuresPos = figuresQueue.Count - 1;
            if (!this.wordsReady || figuresPos < MinimumWordLength - 1)
            {
                return null;
            }

            // Loop while the most recently pressed things are still letters; loop proceeds from the
            // most recent letter, back towards the beginning, as we only care about the longest word
            // that we JUST now finished typing.
            string longestWord = null;
            var stringToCheck = new StringBuilder();
            int lowestIndexToCheck = Math.Max(0, figuresPos - MaximumWordLength);
            while (figuresPos >= lowestIndexToCheck)
            {
                var lastFigure = figuresQueue[figuresPos] as CoolLetter;
                if (lastFigure == null)
                {
                    // If we encounter a non-letter, move on with the best word so far (if any).
                    // IE typing "o [bracket] p e n" can match word "pen" but not "open" since our
                    // intention back at [bracket] shows we don't necessarily mean to type "open".
                    break;
                }

                // Build up the string and check to see if it is a word so far.
                stringToCheck.Insert(0, lastFigure.Character);
                string s = stringToCheck.ToString();
                if (this.words.Contains(stringToCheck.ToString()) && s.Length >= MinimumWordLength)
                {
                    // Since we're progressively checking longer and longer letter combinations,
                    // each time we find a word, it is our new "longest" word so far.
                    longestWord = s;
                }

                figuresPos--;
            }

            return longestWord;
        }

        public void AnimateLettersIntoWord(List<UserControl> figuresQueue, string lastWord)
        {
            // Prepare to animate the letters into their respective positions, on each screen.
            Duration duration = new Duration(TimeSpan.FromMilliseconds(1200));
            int totalLetters = lastWord.Length;

            Point wordCenter = this.FindWordCenter(figuresQueue, totalLetters);
            Point wordSize = this.FindWordSize(figuresQueue, totalLetters);
            double wordLeftEdge = wordCenter.X - wordSize.X / 2f;

            // Figure out where to move each letter used in the word; find the letters used based on
            // the word length; they are the last several figures in the figures queue.
            for (int i = figuresQueue.Count - 1; i >= figuresQueue.Count - totalLetters; i--)
            {
                UserControl currentFigure = figuresQueue[i];

                // Find the translation animation of this element, or make one if there is not one yet.
                var transformGroup = currentFigure.RenderTransform as TransformGroup;
                var transform = FindOrAddTranslationTransform(transformGroup);

                // We know where we want to center the word, and the word's left edge based on figure
                // sizes, and now just need to figure out how far from that left edge we need to adjust
                // to make this letter move to the correct relative position to spell out the word.
                double wordOffsetX = 0d;
                for (int j = figuresQueue.Count - totalLetters; j < i; j++)
                {
                    wordOffsetX += figuresQueue[j].Width;
                }

                // Start translating from wherever we were already translated to (or 0 if not yet
                // translated) and going to the new position for this letter based for the word.
                var wordTranslationX = wordLeftEdge - Canvas.GetLeft(currentFigure);
                var wordTranslationY = wordCenter.Y - Canvas.GetTop(currentFigure);
                var animationX = new DoubleAnimation(transform.X, wordTranslationX + wordOffsetX, duration);
                var animationY = new DoubleAnimation(transform.Y, wordTranslationY, duration);
                transform.BeginAnimation(TranslateTransform.XProperty, animationX);
                transform.BeginAnimation(TranslateTransform.YProperty, animationY);
            }
        }

        private Point FindWordCenter(List<UserControl> letterQueue, int letterCount)
        {
            // For now, target centering the word at the average position of all its letters.
            var x = (from c in letterQueue select Canvas.GetLeft(c)).Reverse().Take(letterCount).Average();
            var y = (from c in letterQueue select Canvas.GetTop(c)).Reverse().Take(letterCount).Average();
            return new Point(x, y);
        }

        private Point FindWordSize(List<UserControl> letterQueue, int letterCount)
        {
            var x = (from c in letterQueue select c.Width).Reverse().Take(letterCount).Sum();
            var y = (from c in letterQueue select c.Height).Reverse().Take(letterCount).Max();
            return new Point(x, y);
        }

        private TranslateTransform FindOrAddTranslationTransform(TransformGroup transformGroup)
        {
            var translationTransform = (from t in transformGroup.Children
                                        where t is TranslateTransform
                                        select t).FirstOrDefault() as TranslateTransform;
            if (translationTransform == null)
            {
                translationTransform = new TranslateTransform();
                transformGroup.Children.Add(translationTransform);
            }

            return translationTransform;
        }
    }
}