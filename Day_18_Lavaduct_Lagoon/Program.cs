using AdventOfCodeUtilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();
HashSet<(int, int)> map = new();
HashSet<(int, int)> dug = new();
HashSet<(int, int)> notDug = new();
int mapMinX = 0;
int mapMinY = 0;
int mapMaxX = 0;
int mapMaxY = 0;

void PrintMap()
{
    for (int y = mapMinY - 2; y <= mapMaxY + 2; y++)
    {
        Console.Write(y.ToString().PadLeft(3, ' ') + " ");
        for (int x = mapMinX - 2; x <= mapMaxX + 2; x++)
        {
            if (dug.Contains((x, y)))
            {
                Console.Write("░");
            }
            else
            {
                Console.Write("█");
            }
        }
        Console.WriteLine();
    }
}

void P1_slow()
{
    int currentX = 0;
    int currentY = 0;
    foreach (string s in inputList)
    {
        var split = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        char dir = split[0][0];
        int distance = int.Parse(split[1]);
        Action mathAct;
        switch (dir)
        {
            case 'R': mathAct = () => currentX++; break;
            case 'L': mathAct = () => currentX--; break;
            case 'U': mathAct = () => currentY--; break;
            case 'D': mathAct = () => currentY++; break;
            default: throw new Exception();
        }
        for (int i = 0; i < distance; i++)
        {
            mathAct();
            dug.Add((currentX, currentY));
        }
    }

    mapMinX = dug.MinBy(tup2 => tup2.Item1).Item1;
    mapMinY = dug.MinBy(tup2 => tup2.Item2).Item2;
    mapMaxX = dug.MaxBy(tup2 => tup2.Item1).Item1;
    mapMaxY = dug.MaxBy(tup2 => tup2.Item2).Item2;

    for (int y = mapMinY; y <= mapMaxY; y++)
    {
        for (int x = mapMinX; x <= mapMaxX; x++)
        {
            (int, int) trial = (x, y);
            map.Add(trial);
        }
    }

    //PrintMap();

    HashSet<(int, int)> newNotDug = new() { (mapMinX, mapMinY), (mapMaxX, mapMinY), (mapMinX, mapMaxY), (mapMaxX, mapMaxY) };
    while (newNotDug.Any())
    {
        var newNotDugCoord = newNotDug.First();
        (int coordX, int coordY) = newNotDugCoord;
        newNotDug.Remove(newNotDugCoord);
        notDug.Add(newNotDugCoord);
        (int, int) trial;

        trial = (coordX - 1, coordY);
        if (coordX > mapMinX)
        {
            if (!dug.Contains(trial) && !notDug.Contains(trial) && !newNotDug.Contains(trial))
                newNotDug.Add(trial);
        }
        trial = (coordX + 1, coordY);
        if (coordX < mapMaxX)
        {
            if (!dug.Contains(trial) && !notDug.Contains(trial) && !newNotDug.Contains(trial))
                newNotDug.Add(trial);
        }
        trial = (coordX, coordY - 1);
        if (coordY > mapMinY)
        {
            if (!dug.Contains(trial) && !notDug.Contains(trial) && !newNotDug.Contains(trial))
                newNotDug.Add(trial);
        }
        trial = (coordX, coordY + 1);
        if (coordY < mapMaxY)
        {
            if (!dug.Contains(trial) && !notDug.Contains(trial) && !newNotDug.Contains(trial))
                newNotDug.Add(trial);
        }
    }


    //PrintMap();

    dug = map.Except(notDug).ToHashSet();
    int result = dug.Count;
    //int result = ((mapMaxY - mapMinY + 1) * (mapMaxX - mapMinX + 1)) - notDug.Count;
    Console.WriteLine(result);
    Console.ReadLine();
}

Int64 cleverSolve(HashSet<(int, int, int)> verticalLines, HashSet<(int, int, int)> horizontalLines)
{
    mapMinX = verticalLines.MinBy(tup3 => tup3.Item1).Item1;
    mapMaxX = verticalLines.MaxBy(tup3 => tup3.Item1).Item1;
    mapMinY = verticalLines.MinBy(tup3 => tup3.Item2).Item2;
    mapMaxY = verticalLines.MaxBy(tup3 => tup3.Item3).Item3;

    Int64 dugCount = 0;
    int lastPerc = -1;
    for (int y = mapMinY; y <= mapMaxY; y++)
    {
        if (Math.Abs(y) % 100_000 == 0)
        {
            int perc = (100 * (y - mapMinY)) / (mapMaxY - mapMinY);
            if (perc != lastPerc)
            {
                int currentLineCursor = Console.CursorTop;
                if (currentLineCursor > 0)
                    Console.SetCursorPosition(0, currentLineCursor - 1);
                Console.WriteLine($"[{"".PadLeft(perc, '=').PadRight(100, ' ')}] {perc.ToString().PadLeft(3)}%");
                lastPerc = perc;
            }
        }
        Int64 rowDugCount = 0;
        var verticalLinesAffectingThisRow = verticalLines.Where(tup3 => tup3.Item2 <= y && tup3.Item3 >= y).OrderBy(tup3 => tup3.Item1).ToList();
        bool inRegion = false;
        for (int i = 0; i < verticalLinesAffectingThisRow.Count - 1; i++)
        {
            int xCoordinateOfThisVerticalLineAffectingThisRow = verticalLinesAffectingThisRow[i].Item1;
            int xCoordinateOfNextVerticalLineAffectingThisRow = verticalLinesAffectingThisRow[i + 1].Item1;

            if (horizontalLines.Contains((y, xCoordinateOfThisVerticalLineAffectingThisRow, xCoordinateOfNextVerticalLineAffectingThisRow)))
            {
                rowDugCount += xCoordinateOfNextVerticalLineAffectingThisRow - xCoordinateOfThisVerticalLineAffectingThisRow;
                if ((verticalLinesAffectingThisRow[i].Item2 == y && verticalLinesAffectingThisRow[i + 1].Item2 == y) ||
                    (verticalLinesAffectingThisRow[i].Item3 == y && verticalLinesAffectingThisRow[i + 1].Item3 == y))
                {
                    inRegion = !inRegion;
                }
            }
            else
            {
                inRegion = !inRegion;
                if (inRegion)
                    rowDugCount += xCoordinateOfNextVerticalLineAffectingThisRow - xCoordinateOfThisVerticalLineAffectingThisRow;
                else
                    rowDugCount++;
            }
        }
        rowDugCount++;
        //Console.WriteLine($"Y:{y} - {dug.Where(tup2 => tup2.Item2 == y).Count()} vs {rowDugCount}");
        dugCount += rowDugCount;
    }

    return dugCount;
}

void P1_fast()
{
    int currentX = 0;
    int currentY = 0;
    HashSet<(int, int, int)> verticalLines = new();
    HashSet<(int, int, int)> horizontalLines = new();
    foreach (string s in inputList)
    {
        var split = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        char dir = split[0][0];
        int distance = int.Parse(split[1]);

        switch (dir)
        {
            case 'R':
                horizontalLines.Add((currentY, currentX, currentX + distance)); currentX += distance; break;
            case 'L':
                horizontalLines.Add((currentY, currentX - distance, currentX)); currentX -= distance; break;
            case 'U':
                verticalLines.Add((currentX, currentY - distance, currentY)); currentY -= distance;
                break;
            case 'D':
                verticalLines.Add((currentX, currentY, currentY + distance)); currentY += distance;
                break;
            default: throw new Exception();
        }
    }

    Int64 dugCount = cleverSolve(verticalLines, horizontalLines);

    Console.WriteLine(dugCount);
    Console.ReadLine();
}

void P2()
{
    int currentX = 0;
    int currentY = 0;
    HashSet<(int, int, int)> verticalLines = new();
    HashSet<(int, int, int)> horizontalLines = new();
    foreach (string s in inputList)
    {
        var split = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string distanceHex = split[2].Substring(2, 5);
        int distance = int.Parse(distanceHex, System.Globalization.NumberStyles.HexNumber);
        char directionHex = split[2].Substring(7, 1)[0];
        char dir;
        switch (directionHex)
        {
            case '0': dir = 'R';break;
            case '1': dir = 'D';break;
            case '2': dir = 'L';break;
            case '3': dir = 'U';break;
            default: throw new Exception();
        }
        
        switch (dir)
        {
            case 'R':
                horizontalLines.Add((currentY, currentX, currentX + distance)); currentX += distance; break;
            case 'L':
                horizontalLines.Add((currentY, currentX - distance, currentX)); currentX -= distance; break;
            case 'U':
                verticalLines.Add((currentX, currentY - distance, currentY)); currentY -= distance;
                break;
            case 'D':
                verticalLines.Add((currentX, currentY, currentY + distance)); currentY += distance;
                break;
            default: throw new Exception();
        }
    }

    Int64 dugCount = cleverSolve(verticalLines, horizontalLines);

    Console.WriteLine(dugCount);
    Console.ReadLine();
}

//P1_slow();
P1_fast();
P2();

public class Cell
{
    bool Dug = false;
    int X;
    int Y;

    public Cell(bool dug = false)
    {
        Dug = dug;
    }
}