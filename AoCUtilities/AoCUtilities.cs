//#define OVERRIDE

using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCodeUtilities
{
    public static class AoC
    {
        public static Int64 GCF(Int64 a, Int64 b)
        {
            while (b != 0)
            {
                Int64 temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public static Int64 LCM(Int64 a, Int64 b)
        {
            return (a / GCF(a, b)) * b;
        }

        static public void DebugClear()
        {
#if DEBUG || OVERRIDE
            Console.Clear();
#endif
        }

        static public string? DebugReadLine()
        {
#if DEBUG || OVERRIDE
            return Console.ReadLine();
#else
            return "";
#endif
        }

        static public void DebugWriteLine()
        {
#if DEBUG || OVERRIDE
            Console.WriteLine();
#endif
        }

        static public void DebugWriteLine(string text, params object[] args)
        {
#if DEBUG || OVERRIDE
            string lineToWrite = string.Format(text, args);
            Console.WriteLine(lineToWrite);
#endif
        }

        static public void DebugWrite(string text, params object[] args)
        {
#if DEBUG || OVERRIDE
            string lineToWrite = string.Format(text, args);
            Console.Write(lineToWrite);
#endif
        }

        static public List<string> GetInputLines(string filename = "input.txt")
        {
            var inputFile = File.ReadAllLines(filename);
            return inputFile.ToList();
        }

        static public string GetInput(string filename = "input.txt")
        {
            var inputFile = File.ReadAllText(filename);
            return inputFile;
        }

        static public MatchCollection RegexMatch(string input, string pattern, bool multiline = false)
        {
            RegexOptions options;
            if (multiline)
                options = RegexOptions.Multiline;
            else
                options = RegexOptions.Singleline;

            return Regex.Matches(input, pattern, options);
        }

        static public double RunWithStopwatch(Action action, Int64 repeats)
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (Int64 i = 0; i < repeats; i++)
                action();
            sw.Stop();
            double elapsedSeconds = (sw.ElapsedTicks / (double)Stopwatch.Frequency);
            double secondsPerRun = elapsedSeconds / repeats;
            return secondsPerRun;
        }

        static public IOrderedEnumerable<TSource> OrderByLambda<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey?, TKey?, int> compareFunc)
        => source.OrderBy(keySelector, new AoCComparer<TKey>(compareFunc, false));

        static public IOrderedEnumerable<TSource> OrderByLambdaDescending<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey?, TKey?, int> compareFunc)
        => source.OrderBy(keySelector, new AoCComparer<TKey>(compareFunc, true));
    }

    internal class AoCComparer<TKey> : IComparer<TKey>
    {
        private readonly Func<TKey?, TKey?, int> _compareFunc;
        private readonly bool _invert;

        public AoCComparer(Func<TKey?, TKey?, int> compareFunc, bool invert)
        {
            _compareFunc = compareFunc;
            _invert = invert;
        }

        public int Compare(TKey? x, TKey? y)
        {
            int result = _compareFunc(x, y);
            return _invert ? result * -1 : result;
        }
    }
}