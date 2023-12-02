using AdventOfCodeUtilities;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();

List<(int, List<(int, int, int)>)> games = inputList.Select(l =>
{
    var split = l.Split(':');
    int gameId = int.Parse(split[0].Split(' ').Last());
    var split2 = split[1].Split(';');
    List<(int, int, int)> rounds = new();
    foreach (var round in split2)
    {
        int blue = 0;
        int red = 0;
        int green = 0;
        var matches = AoC.RegexMatch(round, "(\\d+) (red|blue|green)", false);
        foreach (Match match in matches)
        {
            int count = int.Parse(match.Groups[1].Value);
            string colour = match.Groups[2].Value;
            switch (colour)
            {
                case "blue":
                    blue = count; break;
                case "red":
                    red = count; break;
                case "green":
                    green = count; break;
            }
        }
        rounds.Add((red, green, blue));
    }
    return (gameId, rounds);
}
    ).ToList();

void P1()
{
    int result = 0;
    foreach (var game in games)
    {
        bool gamePossible = true;
        foreach (var round in game.Item2)
        {
            if (round.Item1 > 12 || round.Item2 > 13 || round.Item3 > 14)
            {
                gamePossible = false;
                break;
            }

        }
        if (gamePossible)
            result += game.Item1;
    }
    Console.WriteLine(result);
    Console.ReadLine();
}

void P2()
{
    int result = 0;
    foreach (var game in games)
    {
        int minRed = 0;
        int minGreen = 0;
        int minBlue = 0;
        foreach (var round in game.Item2)
        {
            minRed = Math.Max(minRed, round.Item1);
            minGreen = Math.Max(minGreen, round.Item2);
            minBlue = Math.Max(minBlue, round.Item3);
        }
        result += (minRed * minGreen * minBlue);
    }
    Console.WriteLine(result);
    Console.ReadLine();
}

P1();
P2();