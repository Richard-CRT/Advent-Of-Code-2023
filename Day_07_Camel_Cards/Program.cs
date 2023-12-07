using AdventOfCodeUtilities;
using System.Diagnostics;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();
List<(Hand, int)> pairs = inputList.Select(l =>
{
    var split = l.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    return (new Hand(split[0]), int.Parse(split[1]));
}).ToList();


void P1()
{
    int result = 0;
    var ordered = pairs.OrderBy(pair => pair.Item1, new Compararer(false)).ToList();
    for (int i = 0; i < ordered.Count; i++)
    {
        var pair = ordered[i];
        result += pair.Item2 * (i + 1);
    }
    Console.WriteLine(result);
    Console.ReadLine();
}

void P2()
{
    int result = 0;
    var ordered = pairs.OrderBy(pair => pair.Item1, new Compararer(true)).ToList();
    for (int i = 0; i < ordered.Count; i++)
    {
        var pair = ordered[i];
        result += pair.Item2 * (i + 1);
    }
    Console.WriteLine(result);
    Console.ReadLine();
}

P1();
P2();

public enum Type
{
    FiveOfAKind = 6,
    FourOfAKind = 5,
    FullHouse = 4,
    ThreeOfAKind = 3,
    TwoPair = 2,
    OnePair = 1,
    HighCard = 0,
}

public class Compararer : IComparer<Hand>
{
    private bool Part2 = false;
    public Compararer(bool part2)
    {
        Part2 = part2;
    }

    public int Compare(Hand? x, Hand? y)
    {
        Debug.Assert(x is not null && y is not null);
        if (y.GetType(Part2) > x.GetType(Part2))
            return -1;
        else if (x.GetType(Part2) > y.GetType(Part2))
            return 1;
        else
        {
            for (int i = 0; i < 5; i++)
            {
                int xVal, yVal;
                xVal = !Part2 ? x.Cards[i] : (x.Jokers[i] ? 1 : x.Cards[i]);
                yVal = !Part2 ? y.Cards[i] : (y.Jokers[i] ? 1 : y.Cards[i]);

                if (xVal > yVal)
                    return 1;
                else if (yVal > xVal)
                    return -1;
            }
        }
        return 0;
    }
}

public class Hand
{
    public int[] Cards = new int[5];

    private Type? _type = null;
    private Type? _type2 = null;

    private Type SimpleType(IEnumerable<int> cards)
    {
        var distinctCards = cards.Distinct().ToArray();
        Dictionary<int, int> count = new();
        foreach (int distinctCard in cards)
        {
            count[distinctCard] = cards.Count(c => c == distinctCard);
        }

        if (count.Any(kvp => kvp.Value == 5))
            return Type.FiveOfAKind;
        else if (count.Any(kvp => kvp.Value == 4))
            return Type.FourOfAKind;
        else if (count.Any(kvp => kvp.Value == 3) && count.Any(kvp => kvp.Value == 2))
            return Type.FullHouse;
        else if (count.Any(kvp => kvp.Value == 3))
            return Type.ThreeOfAKind;
        else if (count.Count(kvp => kvp.Value == 2) == 2)
            return Type.TwoPair;
        else if (count.Count(kvp => kvp.Value == 2) == 1)
            return Type.OnePair;
        else
            return Type.HighCard;
    }

    public Type GetType(bool Part2)
    {
        if (!Part2)
        {
            if (_type is null)
            {
                _type = SimpleType(Cards);
            }
            return _type.Value;
        }
        else
        {
            if (_type2 is null)
            {
                var cardsWithoutJokers = Cards.Where(c => c != 11);
                var jokerCount = Jokers.Count(b => b);
                var simpleType = SimpleType(cardsWithoutJokers);

                if (jokerCount == 5)
                    _type2 = Type.FiveOfAKind;
                else
                {
                    _type2 = simpleType;
                    switch (simpleType)
                    {
                        case Type.FourOfAKind:
                            if (jokerCount == 1)
                                _type2 = Type.FiveOfAKind;
                            break;
                        case Type.ThreeOfAKind:
                            if (jokerCount == 2)
                                _type2 = Type.FiveOfAKind;
                            else if (jokerCount == 1)
                                _type2 = Type.FourOfAKind;
                            break;
                        case Type.TwoPair:
                            if (jokerCount == 1)
                                _type2 = Type.FullHouse;
                            break;
                        case Type.OnePair:
                            if (jokerCount == 3)
                                _type2 = Type.FiveOfAKind;
                            else if (jokerCount == 2)
                                _type2 = Type.FourOfAKind;
                            else if (jokerCount == 1)
                                _type2 = Type.ThreeOfAKind;
                            break;
                        case Type.HighCard:
                            if (jokerCount == 4)
                                _type2 = Type.FiveOfAKind;
                            else if (jokerCount == 3)
                                _type2 = Type.FourOfAKind;
                            else if (jokerCount == 2)
                                _type2 = Type.ThreeOfAKind;
                            else if (jokerCount == 1)
                                _type2 = Type.OnePair;
                            break;
                    }
                }
            }

            return _type2.Value;
        }
    }

    public bool[] Jokers = new bool[5];

    public Hand(string handString)
    {
        int i = 0;
        Cards = handString.Trim().Select(c =>
        {
            Jokers[i] = c == 'J' ? true : false;
            i++;
            switch (c)
            {
                case 'A': return 14;
                case 'K': return 13;
                case 'Q': return 12;
                case 'J': return 11;
                case 'T': return 10;
                default: return int.Parse($"{c}");
            }
        }).ToArray();
    }

    public override string ToString()
    {
        return string.Join(' ', Cards);
    }


}