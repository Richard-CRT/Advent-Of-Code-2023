﻿//#define OVERRIDE

using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCodeUtilities
{
    public static class AoC
    {
        public static int GCF(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public static int LCM(int a, int b)
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

        public static IOrderedEnumerable<TSource> OrderByLambda<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey?, TKey?, int> compareFunc)
        {
            var comparer = new AoCComparer<TKey>(compareFunc, false);
            return source.OrderBy(keySelector, comparer);
        }

        public static IOrderedEnumerable<TSource> OrderByLambdaDescending<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey?, TKey?, int> compareFunc)
        {
            var comparer = new AoCComparer<TKey>(compareFunc, true);
            return source.OrderBy(keySelector, comparer);
        }
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