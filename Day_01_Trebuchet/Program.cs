using AdventOfCodeUtilities;
using System.Collections.Generic;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();

void P1()
{
    var nums = inputList.Select(l => {
        var t = l.ToList().Where(c => c >= '0' && c <= '9');
        return Int32.Parse($"{t.First()}{t.Last()}");
    });
    Console.WriteLine(nums.Sum());
    Console.ReadLine();
}

void P2()
{
    Dictionary<string, int> dict = new Dictionary<string, int>() {
        { "0", 0 },
        { "1", 1 },
        { "2", 2 },
        { "3", 3 },
        { "4", 4 },
        { "5", 5 },
        { "6", 6 },
        { "7", 7 },
        { "8", 8 },
        { "9", 9 },
        { "one", 1 },
        { "two", 2 },
        { "three", 3 },
        { "four", 4 },
        { "five", 5 },
        { "six", 6 },
        { "seven", 7 },
        { "eight", 8 },
        { "nine", 9 },
        { "zero", 0 },
    };
    var nums = inputList.Select(l => {
        var matches = AoC.RegexMatch(l, "(one|two|three|four|five|six|seven|eight|nine|\\d).*(one|two|three|four|five|six|seven|eight|nine|\\d)", false);
        if (matches.Count == 0)
        {
            var matches1 = AoC.RegexMatch(l, "(one|two|three|four|five|six|seven|eight|nine|\\d)", false);
            string first = matches1[0].Groups[1].Value;
            return Int32.Parse($"{dict[first]}{dict[first]}");
        }
        else
        {
            string first = matches[0].Groups[1].Value;
            string second = matches[0].Groups[2].Value;

            return Int32.Parse($"{dict[first]}{dict[second]}");
        }
    });
    Console.WriteLine(nums.Sum());
    Console.ReadLine();
}

P1();
P2();