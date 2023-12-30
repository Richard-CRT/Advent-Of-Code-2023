using AdventOfCodeUtilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();

UInt128 one = 1;

List<List<Cell>> map = new();
List<Cell> cells = new();

for (int y = 0; y < inputList.Count; y++)
{
    map.Add(new());
    for (int x = 0; x < inputList[y].Length; x++)
    {
        Cell newCell = new(x, y, inputList[y][x]);
        cells.Add(newCell);
        map[y].Add(newCell);
    }
}

Cell startCell = map[0][1];
Cell endCell = map[^1][^2];

cells.ForEach(cell => cell.FindNeighboursP1(map));
cells.ForEach(cell => cell.FindNeighboursP2(map));
cells.ForEach(cell => cell.FindSegmentHubs());
cells.ForEach(cell => cell.FindSegment());

cells.ForEach(cell =>
{
    if (cell.Type != CellType.Forest)
    {
        cell.NeighboursP1.ForEach((neighbourCell) =>
        {
            if (cell.PathSegment! != neighbourCell.PathSegment!)
            {
                cell.PathSegment.NeighbourP1Segments.Add(neighbourCell.PathSegment);
            }
        });
        cell.NeighboursP2.ForEach((neighbourCell) =>
        {
            if (cell.PathSegment! != neighbourCell.PathSegment!)
            {
                cell.PathSegment.NeighbourP2Segments.Add(neighbourCell.PathSegment);
                neighbourCell.PathSegment.NeighbourP2Segments.Add(cell.PathSegment);
            }
        });
    }
});

/*
for (int y = 0; y < map.Count; y++)
{
    for (int x = 0; x < map[y].Count; x++)
    {
        if (map[y][x].Type != CellType.Forest)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(map[y][x].PathSegment!.HumanReadableIndex);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write((char)map[y][x].Type);
        }
    }
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine();
}
//Console.ReadLine();
*/

int recurseP1(HashSet<Cell> pathSoFar, Cell head, Cell endCell)
{
    if (head == endCell)
    {
        return pathSoFar.Count - 1;
    }
    int maxLength = -1;
    foreach (Cell neighbour in head.NeighboursP1)
    {
        if (!pathSoFar.Contains(neighbour))
        {
            HashSet<Cell> copyOfPathSoFar = new(pathSoFar);
            copyOfPathSoFar.Add(neighbour);
            maxLength = Math.Max(maxLength, recurseP1(copyOfPathSoFar, neighbour, endCell));
        }
    }
    return maxLength;
}

int recurseP1Segments(HashSet<PathSegment> segmentsSoFar, PathSegment head, PathSegment endSegment)
{
    if (head == endSegment)
    {
        //Console.WriteLine(string.Join(' ', segmentsSoFar.Select(segment => segment.HumanReadableIndex)));
        int length = segmentsSoFar.Sum(segment => segment.Cells.Count) - 1;
        return length;
    }
    int maxLength = -1;
    foreach (PathSegment neighbourSegment in head.NeighbourP1Segments)
    {
        if (!segmentsSoFar.Contains(neighbourSegment))
        {
            HashSet<PathSegment> copyOfSegmentsSoFar = new(segmentsSoFar);
            copyOfSegmentsSoFar.Add(neighbourSegment);
            maxLength = Math.Max(maxLength, recurseP1Segments(copyOfSegmentsSoFar, neighbourSegment, endSegment));
        }
    }
    return maxLength;
}

int recurseP1SegmentsWithBitwiseHistory(UInt128 segmentsSoFarBitwise, PathSegment head, PathSegment endSegment)
{
    if (head == endSegment)
    {
        int length = 0;
        UInt128 segmentBitMask = one << 0;
        for (int index = 0; index < PathSegment.OverallIndex; index++, segmentBitMask <<= 1)
        {
            if ((segmentsSoFarBitwise & segmentBitMask) != 0)
                length += PathSegment.PathSegmentsByIndex[index].Cells.Count;
        }
        length--;

        return length;
    }
    int maxLength = -1;
    foreach (PathSegment neighbourSegment in head.NeighbourP1Segments)
    {
        UInt128 neighbourSegmentBitMask = one << neighbourSegment.Index;

        if ((segmentsSoFarBitwise & neighbourSegmentBitMask) == 0)
        {
            UInt128 newSegmentsSoFarBitwise = segmentsSoFarBitwise | neighbourSegmentBitMask;
            maxLength = Math.Max(maxLength, recurseP1SegmentsWithBitwiseHistory(newSegmentsSoFarBitwise, neighbourSegment, endSegment));
        }
    }
    return maxLength;
}

int recurseP1SegmentsWithBoolArrHistory(bool[] segmentIndexesSoFar, PathSegment head, PathSegment endSegment)
{
    if (head == endSegment)
    {
        int length = 0;
        for (int i = 0; i < segmentIndexesSoFar.Length; i++)
        {
            if (segmentIndexesSoFar[i])
                length += PathSegment.PathSegmentsByIndex[i].Cells.Count;
        }
        length--;
        return length;
    }
    int maxLength = -1;
    foreach (PathSegment neighbourSegment in head.NeighbourP1Segments)
    {
        if (!segmentIndexesSoFar[neighbourSegment.Index])
        {
            bool[] copyOfSegmentIndexesSoFar = new bool[segmentIndexesSoFar.Length];
            segmentIndexesSoFar.CopyTo(copyOfSegmentIndexesSoFar, 0);
            copyOfSegmentIndexesSoFar[neighbourSegment.Index] = true;
            maxLength = Math.Max(maxLength, recurseP1SegmentsWithBoolArrHistory(copyOfSegmentIndexesSoFar, neighbourSegment, endSegment));
        }
    }
    return maxLength;
}

int recurseP2(HashSet<Cell> pathSoFar, Cell head, Cell endCell)
{
    if (head == endCell)
    {
        int length = pathSoFar.Count - 1;
        //Console.WriteLine(length);
        return length;
    }
    int maxLength = -1;
    foreach (Cell neighbour in head.NeighboursP2)
    {
        if (!pathSoFar.Contains(neighbour))
        {
            HashSet<Cell> copyOfPathSoFar = new(pathSoFar);
            copyOfPathSoFar.Add(neighbour);
            maxLength = Math.Max(maxLength, recurseP2(copyOfPathSoFar, neighbour, endCell));
        }
    }
    return maxLength;
}

int recurseP2Segments(HashSet<PathSegment> segmentsSoFar, PathSegment head, PathSegment endSegment)
{
    if (head == endSegment)
    {
        //Console.WriteLine(string.Join(' ', segmentsSoFar.Select(segment => segment.HumanReadableIndex)));
        int length = segmentsSoFar.Sum(segment => segment.Cells.Count) - 1;
        return length;
    }
    int maxLength = -1;
    foreach (PathSegment neighbourSegment in head.NeighbourP2Segments)
    {
        if (!segmentsSoFar.Contains(neighbourSegment))
        {
            HashSet<PathSegment> copyOfSegmentsSoFar = new(segmentsSoFar);
            copyOfSegmentsSoFar.Add(neighbourSegment);
            maxLength = Math.Max(maxLength, recurseP2Segments(copyOfSegmentsSoFar, neighbourSegment, endSegment));
        }
    }
    return maxLength;
}

int recurseP2SegmentsWithBoolArrHistory(bool[] segmentIndexesSoFar, PathSegment head, PathSegment endSegment)
{
    if (head == endSegment)
    {
        int length = 0;
        for (int i = 0; i < segmentIndexesSoFar.Length; i++)
        {
            if (segmentIndexesSoFar[i])
                length += PathSegment.PathSegmentsByIndex[i].Cells.Count;
        }
        length--;
        return length;
    }
    int maxLength = -1;
    foreach (PathSegment neighbourSegment in head.NeighbourP2Segments)
    {
        if (!segmentIndexesSoFar[neighbourSegment.Index])
        {
            bool[] copyOfSegmentIndexesSoFar = new bool[segmentIndexesSoFar.Length];
            segmentIndexesSoFar.CopyTo(copyOfSegmentIndexesSoFar, 0);
            copyOfSegmentIndexesSoFar[neighbourSegment.Index] = true;
            maxLength = Math.Max(maxLength, recurseP2SegmentsWithBoolArrHistory(copyOfSegmentIndexesSoFar, neighbourSegment, endSegment));
        }
    }
    return maxLength;
}

int recurseP2SegmentsWithBitwiseHistory(UInt128 segmentsSoFarBitwise, PathSegment head, PathSegment endSegment)
{
    if (head == endSegment)
    {
        int length = 0;
        UInt128 segmentBitMask = one << 0;
        for (int index = 0; index < PathSegment.OverallIndex; index++, segmentBitMask <<= 1)
        {
            if ((segmentsSoFarBitwise & segmentBitMask) != 0)
                length += PathSegment.PathSegmentsByIndex[index].Cells.Count;
        }
        length--;

        return length;
    }
    int maxLength = -1;
    foreach (PathSegment neighbourSegment in head.NeighbourP2Segments)
    {
        UInt128 neighbourSegmentBitMask = one << neighbourSegment.Index;

        if ((segmentsSoFarBitwise & neighbourSegmentBitMask) == 0)
        {
            UInt128 newSegmentsSoFarBitwise = segmentsSoFarBitwise | neighbourSegmentBitMask;
            maxLength = Math.Max(maxLength, recurseP2SegmentsWithBitwiseHistory(newSegmentsSoFarBitwise, neighbourSegment, endSegment));
        }
    }
    return maxLength;
}

void P1()
{
    int result = 0;

    //result = recurseP1(new HashSet<Cell> { startCell }, startCell, endCell);
    //result = recurseP1Segments(new HashSet<PathSegment> { startCell.PathSegment! }, startCell.PathSegment!, endCell.PathSegment!);
    result = recurseP1SegmentsWithBitwiseHistory(one << startCell.PathSegment!.Index, startCell.PathSegment!, endCell.PathSegment!);

    bool[] boolHistory = new bool[PathSegment.OverallIndex];
    boolHistory[startCell.PathSegment!.Index] = true;
    //result = recurseP1SegmentsWithBoolArrHistory(boolHistory, startCell.PathSegment!, endCell.PathSegment!);

    // Bool array history is a bit slower than bitwise history but bool array is easier to understand and not limited to 128 nodes

    Console.WriteLine(result);
    Console.ReadLine();
}

void P2()
{
    int result = 0;

    //result = recurseP2(new HashSet<Cell> { startCell }, startCell, endCell);
    //result = recurseP2Segments(new HashSet<PathSegment> { startCell.PathSegment! }, startCell.PathSegment!, endCell.PathSegment!);
    result = recurseP2SegmentsWithBitwiseHistory(one << startCell.PathSegment!.Index, startCell.PathSegment!, endCell.PathSegment!);

    bool[] boolHistory = new bool[PathSegment.OverallIndex];
    boolHistory[startCell.PathSegment!.Index] = true;
    //result = recurseP2SegmentsWithBoolArrHistory(boolHistory, startCell.PathSegment!, endCell.PathSegment!);

    // Bool array history is a bit slower than bitwise history but bool array is easier to understand and not limited to 128 nodes

    Console.WriteLine(result);
    Console.ReadLine();
}

P1();
P2();

public enum CellType
{
    Forest = '#',
    Path = '.',
    EastSlope = '>',
    SouthSlope = 'v',
    WestSlope = '<',
    NorthSlope = '^',
}

public class PathSegment
{
    public static int OverallIndex = 0;
    public static Dictionary<int, PathSegment> PathSegmentsByIndex = new();

    public HashSet<PathSegment> NeighbourP1Segments = new();
    public HashSet<PathSegment> NeighbourP2Segments = new();
    public HashSet<Cell> Cells = new();
    public char HumanReadableIndex;
    public int Index;

    public PathSegment()
    {
        Index = OverallIndex;
        HumanReadableIndex = (char)('A' + OverallIndex);
        PathSegmentsByIndex[Index] = this;

        OverallIndex++;
    }

    public override string ToString()
    {
        return $"(PathSeg I:{HumanReadableIndex} Len:{Cells.Count})";
    }
}

public class Cell
{
    public int X;
    public int Y;
    public CellType Type;
    public List<Cell> NeighboursP1 = new();
    public List<Cell> NeighboursP2 = new();
    public bool FoundSegment = false;
    public PathSegment? PathSegment = null;

    public Cell(int x, int y, char c)
    {
        X = x;
        Y = y;
        Type = (CellType)c;
    }

    public override string ToString()
    {
        return $"(X:{X} Y:{Y} T:{Type}";
    }

    public void FindNeighboursP1(List<List<Cell>> map)
    {
        if (Type != CellType.Forest)
        {
            switch (Type)
            {
                case CellType.Path:
                    {
                        foreach ((int _x, int _y) in new List<(int, int)>() { (-1, 0), (0, -1), (1, 0), (0, 1) })
                        {
                            int trialX = X + _x;
                            int trialY = Y + _y;
                            if (trialY >= 0 && trialY < map.Count && trialX >= 0 && trialX < map[trialY].Count && map[trialY][trialX].Type != CellType.Forest)
                            {
                                switch (map[trialY][trialX].Type)
                                {
                                    case CellType.EastSlope:
                                        if (_x == 1)
                                            NeighboursP1.Add(map[trialY][trialX]);
                                        break;
                                    case CellType.WestSlope:
                                        if (_x == -1)
                                            NeighboursP1.Add(map[trialY][trialX]);
                                        break;
                                    case CellType.NorthSlope:
                                        if (_y == -1)
                                            NeighboursP1.Add(map[trialY][trialX]);
                                        break;
                                    case CellType.SouthSlope:
                                        if (_y == 1)
                                            NeighboursP1.Add(map[trialY][trialX]);
                                        break;
                                    default:
                                        NeighboursP1.Add(map[trialY][trialX]);
                                        break;
                                }

                            }
                        }
                        break;
                    }
                case CellType.EastSlope:
                    {
                        int trialX = X + 1;
                        int trialY = Y;
                        if (trialY >= 0 && trialY < map.Count && trialX >= 0 && trialX < map[trialY].Count && map[trialY][trialX].Type != CellType.Forest)
                        {
                            NeighboursP1.Add(map[trialY][trialX]);
                        }
                        break;
                    }
                case CellType.WestSlope:
                    {
                        int trialX = X - 1;
                        int trialY = Y;
                        if (trialY >= 0 && trialY < map.Count && trialX >= 0 && trialX < map[trialY].Count && map[trialY][trialX].Type != CellType.Forest)
                        {
                            NeighboursP1.Add(map[trialY][trialX]);
                        }
                        break;
                    }
                case CellType.NorthSlope:
                    {
                        int trialX = X;
                        int trialY = Y - 1;
                        if (trialY >= 0 && trialY < map.Count && trialX >= 0 && trialX < map[trialY].Count && map[trialY][trialX].Type != CellType.Forest)
                        {
                            NeighboursP1.Add(map[trialY][trialX]);
                        }
                        break;
                    }
                case CellType.SouthSlope:
                    {
                        int trialX = X;
                        int trialY = Y + 1;
                        if (trialY >= 0 && trialY < map.Count && trialX >= 0 && trialX < map[trialY].Count && map[trialY][trialX].Type != CellType.Forest)
                        {
                            NeighboursP1.Add(map[trialY][trialX]);
                        }
                        break;
                    }
            }
        }
    }

    public void FindNeighboursP2(List<List<Cell>> map)
    {
        if (Type != CellType.Forest)
        {
            switch (Type)
            {
                case CellType.Path:
                case CellType.SouthSlope:
                case CellType.NorthSlope:
                case CellType.WestSlope:
                case CellType.EastSlope:
                    {
                        foreach ((int _x, int _y) in new List<(int, int)>() { (-1, 0), (0, -1), (1, 0), (0, 1) })
                        {
                            int trialX = X + _x;
                            int trialY = Y + _y;
                            if (trialY >= 0 && trialY < map.Count && trialX >= 0 && trialX < map[trialY].Count && map[trialY][trialX].Type != CellType.Forest)
                            {
                                NeighboursP2.Add(map[trialY][trialX]);
                            }
                        }
                        break;
                    }
            }
        }
    }


    public void FindSegmentHubs()
    {
        if (this.NeighboursP2.Count > 2)
        {
            // this is a hub
            PathSegment newPathSegment = new();
            PathSegment = newPathSegment;
            newPathSegment.Cells.Add(this);
        }
    }

    public void FindSegment()
    {
        var neighboursWithoutASegmentYet = NeighboursP2.Where(neighbour => neighbour.PathSegment is null).ToList();
        if (PathSegment is null && neighboursWithoutASegmentYet.Count == 1)
        {
            PathSegment newPathSegment = new();
            PathSegment = newPathSegment;
            //bfs to find cells in this segment
            Queue<Cell> searchCells = new();
            searchCells.Enqueue(this);
            while (searchCells.Any())
            {
                Cell searchCell = searchCells.Dequeue();

                newPathSegment.Cells.Add(searchCell);
                searchCell.PathSegment = newPathSegment;

                var searchCellNeighboursWithoutASegmentYet = searchCell.NeighboursP2.Where(neighbour => neighbour.PathSegment is null).ToList();
                if (searchCellNeighboursWithoutASegmentYet.Count == 1)
                {
                    searchCells.Enqueue(searchCellNeighboursWithoutASegmentYet.First());
                }
            }
        }
    }
}