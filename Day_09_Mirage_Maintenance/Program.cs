using AdventOfCodeUtilities;
using System.Diagnostics;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();

List<int> diff(List<int> input)
{
    List<int> result = new();
    for (int i = 0; i < input.Count - 1; i++)
    {
        result.Add(input[i + 1] - input[i]);
    }
    return result;
}

List<List<int>> valueSets = inputList.Select(l => l.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToList()).ToList();

void P1and2()
{
    // P2 can be easily achieved by reversing the input lines, but may as well do it at the same time like this
    List<List<int>> extrapolated = new();
    foreach (var valueSet in valueSets)
    {
        List<int> workingCopy = new List<int>(valueSet);
        List<List<int>> levels = new() { workingCopy };
        while (workingCopy.Distinct().Count() > 1)
        {
            workingCopy = diff(workingCopy);
            levels.Add(workingCopy);
        }
        for (int i = levels.Count - 1; i > 0; i--)
        {
            levels[i - 1].Add(levels[i - 1].Last() + levels[i].Last());
            levels[i - 1].Insert(0, levels[i - 1].First() - levels[i].First());
        }
        extrapolated.Add(levels[0]);
    }
    int resultP1 = extrapolated.Sum(l => l.Last());
    Console.WriteLine(resultP1);
    Console.ReadLine();
    int resultP2 = extrapolated.Sum(l => l.First());
    Console.WriteLine(resultP2);
    Console.ReadLine();
}

P1and2();