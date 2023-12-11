using AdventOfCodeUtilities;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();
List<(int, int)> mapGalaxyCoords = new();
List<(int, int)> expandedMapGalaxyCoords = new();
List<List<Type>> map = inputList.Select(l => l.Select(c => (Type)c).ToList()).ToList();

HashSet<int> columnsThatExpand = new HashSet<int>();
HashSet<int> rowsThatExpand = new HashSet<int>();

for (int y = 0; y < map.Count; y++)
{
    bool allSpace = true;
    for (int x = 0; allSpace && x < map[y].Count; x++)
    {
        if (map[y][x] != Type.Space)
            allSpace = false;
    }
    if (allSpace)
    {
        rowsThatExpand.Add(y);
    }
}

for (int x = 0; x < map[0].Count; x++)
{
    bool allSpace = true;
    for (int y = 0; allSpace && y < map.Count; y++)
    {
        if (map[y][x] != Type.Space)
            allSpace = false;
    }
    if (allSpace)
    {
        columnsThatExpand.Add(x);
    }
}

List<List<Type>> expandedMap = inputList.Select(l => l.Select(c => (Type)c).ToList()).ToList();

for (int y = 0; y < expandedMap.Count; y++)
{
    bool allSpace = true;
    for (int x = 0; allSpace && x < expandedMap[y].Count; x++)
    {
        if (expandedMap[y][x] != Type.Space)
            allSpace = false;
    }
    if (allSpace)
    {
        expandedMap.Insert(y, new List<Type>(expandedMap[y]));
        y++;
    }
}

for (int x = 0; x < expandedMap[0].Count; x++)
{
    bool allSpace = true;
    for (int y = 0; allSpace && y < expandedMap.Count; y++)
    {
        if (expandedMap[y][x] != Type.Space)
            allSpace = false;
    }
    if (allSpace)
    {
        for (int y = 0; y < expandedMap.Count; y++)
        {
            expandedMap[y].Insert(x, Type.Space);
        }
        x++;
    }
}

for (int y = 0; y < expandedMap.Count; y++)
{
    for (int x = 0; x < expandedMap[y].Count; x++)
    {
        if (expandedMap[y][x] == Type.Galaxy)
            expandedMapGalaxyCoords.Add((x, y));
    }
}

for (int y = 0; y < map.Count; y++)
{
    for (int x = 0; x < map[y].Count; x++)
    {
        if (map[y][x] == Type.Galaxy)
            mapGalaxyCoords.Add((x, y));
    }
}

void P1()
{
    int result = 0;

    for (int i = 0; i < expandedMapGalaxyCoords.Count; i++)
    {
        (int galaxy1X, int galaxy1Y) = expandedMapGalaxyCoords[i];

        int shortestDistance = int.MaxValue;
        for (int j = i + 1; j < expandedMapGalaxyCoords.Count; j++)
        {
            (int galaxy2X, int galaxy2Y) = expandedMapGalaxyCoords[j];

            if (galaxy1X != galaxy2X || galaxy1Y != galaxy2Y)
            {
                int distance = Math.Abs(galaxy2X - galaxy1X) + Math.Abs(galaxy2Y - galaxy1Y);
                shortestDistance = Math.Min(shortestDistance, distance);
                result += distance;
            }
        }
        //result += shortestDistance;
    }

    Console.WriteLine(result);
    Console.ReadLine();
}

void P2()
{
    Int64 result = 0;

    for (int i = 0; i < mapGalaxyCoords.Count; i++)
    {
        (int galaxy1X, int galaxy1Y) = mapGalaxyCoords[i];

        Int64 shortestDistance = int.MaxValue;
        for (int j = i + 1; j < mapGalaxyCoords.Count; j++)
        {
            (int galaxy2X, int galaxy2Y) = mapGalaxyCoords[j];

            if (galaxy1X != galaxy2X || galaxy1Y != galaxy2Y)
            {
                Int64 xDiff = Math.Abs(galaxy2X - galaxy1X);
                Int64 yDiff = Math.Abs(galaxy2Y - galaxy1Y);
                for (int _x = Math.Min(galaxy1X, galaxy2X) + 1; _x <= Math.Max(galaxy1X, galaxy2X) - 1; _x++)
                {
                    if (columnsThatExpand.Contains(_x))
                        xDiff += 1_000_000 - 1;
                }
                for (int _y = Math.Min(galaxy1Y, galaxy2Y) + 1; _y <= Math.Max(galaxy1Y, galaxy2Y) - 1; _y++)
                {
                    if (rowsThatExpand.Contains(_y))
                        yDiff += 1_000_000 - 1;
                }
                Int64 distance = xDiff + yDiff;
                shortestDistance = Math.Min(shortestDistance, distance);
                result += distance;
            }
        }
        //result += shortestDistance;
    }

    Console.WriteLine(result);
    Console.ReadLine();
}

P1();
P2();

public enum Type
{
    Space = '.',
    Galaxy = '#'
}