using AdventOfCodeUtilities;
using System.Diagnostics;
using System.Text.RegularExpressions;
// ┘┐┌└█▓▒░
List<string> inputList = AoC.GetInputLines();
List<Cell> cells = new();
List<List<Cell>> map = new();

for (int y = 0; y < inputList.Count; y++)
{
    List<Cell> row = new();
    for (int x = 0; x < inputList[y].Length; x++)
    {
        var newCell = new Cell(x, y, inputList[y][x]);
        cells.Add(newCell);
        row.Add(newCell);
    }
    map.Add(row);
}

#pragma warning disable CS8321
void PrintMap()
{
    for (int y = 0; y < map.Count; y++)
    {
        for (int x = 0; x < map[y].Count; x++)
        {
            if (map[y][x].Energised)
                Console.Write("▒");
            else
                Console.Write((char)map[y][x].Type);
        }
        Console.WriteLine();
    }
}

int TrialLocation((int, int, Direction) initialBeamHead)
{
    cells.ForEach(cell => cell.Reset());
    List<(int, int, Direction)> beamHeads = new() { initialBeamHead };
    int energisedCells = 0;
    bool change = true;
    while (change)
    {
        change = false;
        List<(int, int, Direction)> newBeamHeads = new();
        foreach ((int beamHeadX, int beamHeadY, Direction beamDirection) in beamHeads)
        {
            int newBeamHeadX = beamHeadX;
            int newBeamHeadY = beamHeadY;
            switch (beamDirection)
            {
                case Direction.East: newBeamHeadX = beamHeadX + 1; break;
                case Direction.West: newBeamHeadX = beamHeadX - 1; break;
                case Direction.North: newBeamHeadY = beamHeadY - 1; break;
                case Direction.South: newBeamHeadY = beamHeadY + 1; break;
            }
            if (newBeamHeadY >= 0 && newBeamHeadY < map.Count && newBeamHeadX >= 0 && newBeamHeadX < map[newBeamHeadY].Count)
            {
                (IEnumerable<Direction> toDirections, bool newEnergised) = map[newBeamHeadY][newBeamHeadX].Beam(fromDirection: beamDirection);
                if (toDirections.Any())
                    change = true;
                if (newEnergised)
                    energisedCells++;
                foreach (var toDirection in toDirections)
                {
                    newBeamHeads.Add((newBeamHeadX, newBeamHeadY, toDirection));
                }
            }
        }
        beamHeads = newBeamHeads;
    }
    return energisedCells;
}

void P1()
{
    int energisedCells = TrialLocation((-1, 0, Direction.East));
    Console.WriteLine(energisedCells);
    Console.ReadLine();
}

void P2()
{
    int maxEnergisedCells = 0;
    for (int x = 0; x < map[0].Count; x++)
    {
        int energisedCells = TrialLocation((x, -1, Direction.South));
        maxEnergisedCells = Math.Max(maxEnergisedCells, energisedCells);
        energisedCells = TrialLocation((x, map.Count, Direction.North));
        maxEnergisedCells = Math.Max(maxEnergisedCells, energisedCells);
    }
    for (int y = 0; y < map.Count; y++)
    {
        int energisedCells = TrialLocation((-1, y, Direction.East));
        maxEnergisedCells = Math.Max(maxEnergisedCells, energisedCells);
        energisedCells = TrialLocation((map[y].Count, y, Direction.West));
        maxEnergisedCells = Math.Max(maxEnergisedCells, energisedCells);
    }

    Console.WriteLine(maxEnergisedCells);
    Console.ReadLine();
}

P1();
P2();

public enum CellType
{
    Space = '.',
    VerticalSplitter = '|',
    HorizontalSplitter = '-',
    TLBRMirror = '\\',
    TRBLMirror = '/',
}

public enum Direction
{
    North,
    West,
    South,
    East,
}

public class Cell
{
    public int X;
    public int Y;
    public CellType Type;
    public bool Energised = false;
    public HashSet<Direction> AlreadyExperiencedToDirections = new();

    public Cell(int x, int y, char c)
    {
        X = x;
        Y = y;
        Type = (CellType)c;
    }

    public (IEnumerable<Direction>, bool) Beam(Direction fromDirection)
    {
        bool newEnergised = Energised == false;
        Energised = true;
        HashSet<Direction> toDirections = new();
        switch (this.Type)
        {
            case CellType.Space: toDirections.Add(fromDirection); break;
            case CellType.TLBRMirror:
                switch (fromDirection)
                {
                    case Direction.North: toDirections.Add(Direction.West); break;
                    case Direction.South: toDirections.Add(Direction.East); break;
                    case Direction.West: toDirections.Add(Direction.North); break;
                    case Direction.East: toDirections.Add(Direction.South); break;
                }
                break;
            case CellType.TRBLMirror:
                switch (fromDirection)
                {
                    case Direction.North: toDirections.Add(Direction.East); break;
                    case Direction.South: toDirections.Add(Direction.West); break;
                    case Direction.West: toDirections.Add(Direction.South); break;
                    case Direction.East: toDirections.Add(Direction.North); break;
                }
                break;
            case CellType.HorizontalSplitter:
                switch (fromDirection)
                {
                    case Direction.North:
                    case Direction.South:
                        toDirections.Add(Direction.West);
                        toDirections.Add(Direction.East);
                        break;
                    case Direction.West:
                    case Direction.East: toDirections.Add(fromDirection); break;
                }
                break;
            case CellType.VerticalSplitter:
                switch (fromDirection)
                {
                    case Direction.North:
                    case Direction.South: toDirections.Add(fromDirection); break;
                    case Direction.West:
                    case Direction.East:
                        toDirections.Add(Direction.North);
                        toDirections.Add(Direction.South);
                        break;
                }
                break;
        }
        toDirections = toDirections.Except(AlreadyExperiencedToDirections).ToHashSet();
        AlreadyExperiencedToDirections = AlreadyExperiencedToDirections.Union(toDirections).ToHashSet();
        return (toDirections, newEnergised);
    }

    public void Reset()
    {
        Energised = false;
        AlreadyExperiencedToDirections.Clear();
    }
}