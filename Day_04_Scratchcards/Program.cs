using AdventOfCodeUtilities;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();

var multiples = new Dictionary<int, int>();
var cards = new Dictionary<int, (HashSet<int>, HashSet<int>)>(inputList.Select(l => {
    var split = l.Split('|');
    var split2 = split[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var winning = split2[2..].Select(sn => int.Parse(sn)).ToHashSet();
    var youHave = split[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(sn => int.Parse(sn)).ToHashSet();
    int id = int.Parse(split2[1][..^1]);
    multiples[id] = 1;
    return new KeyValuePair<int, (HashSet<int>, HashSet<int>)>(id, (winning, youHave));
}));

void P1()
{
    int result = 0;
    foreach (var card in cards)
    {
        int val = 0;
        foreach (var num in card.Value.Item2)
        {
            if (card.Value.Item1.Contains(num))
            {
                if (val == 0)
                    val = 1;
                else val *= 2;
            }
        }
        result += val;
    }
    Console.WriteLine(result);
    Console.ReadLine();
}

void P2()
{
    for (int i = 1; i <= cards.Count; i++)
    {
        var card = cards[i];
        int numMatch = 0;
        foreach (var num in card.Item2)
        {
            if (card.Item1.Contains(num))
            {
                numMatch++;
            }
        }
        for (int n = 1; n <= numMatch; n++)
        {
            multiples[i + n] += multiples[i];
        }
    }
    Console.WriteLine(multiples.Sum(kVP => kVP.Value));
    Console.ReadLine();
}

P1();
P2();