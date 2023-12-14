using AdventOfCodeUtilities;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();
List<Cell> roundRocks = new();
List<Cell> roundRocksP2 = new();
int y = -1;
int x = -1;
List<List<Cell?>> map = inputList.Select(line =>
{
    x = -1;
    y++;
    return line.Select(c =>
    {
        x++;
        if (c == '.')
            return null;
        else
        {
            Cell newCell = new(c, x, y);
            if (newCell.CellType == CellType.Rock)
            {
                roundRocks.Add(newCell);
            }
            return newCell;
        }
    }).ToList();
}).ToList();

y = -1;
List<List<Cell?>> mapP2 = inputList.Select(line =>
{
    x = -1;
    y++;
    return line.Select(c =>
    {
        x++;
        if (c == '.')
            return null;
        else
        {
            Cell newCell = new(c, x, y);
            if (newCell.CellType == CellType.Rock)
            {
                roundRocksP2.Add(newCell);
            }
            return newCell;
        }
    }).ToList();
}).ToList();
int mapHeight = map.Count;
int mapWidth = map[0].Count;

string GetStringRep(List<List<Cell?>> map)
{
    string s = "";
    for (int y = 0; y < map.Count; y++)
    {
        s += $"{new string(map[y].Select(cell => (cell is null ? '.' : (char)cell.CellType)).ToArray())}\n";
        /*
        for (int x = 0; x < map[y].Count; x++)
        {
            if (map[y][x] is null)
                s += ".";
            else
            {
                if (map[y][x]!.CellType == CellType.Rock)
                    s += "O";
                else
                    s += "#";
            }
        }
        s += "\n";
        */
    }
    return s;
}

void PrintMap(List<List<Cell?>> map)
{
    Console.WriteLine($"{GetStringRep(map)}\n");
}

void P1()
{
    foreach (var roundRock in roundRocks)
    {
        roundRock.RollNorth(map);
    }
    int result = roundRocks.Sum(roundRock => roundRock.CalculateLoad(mapHeight));
    Console.WriteLine(result);
    Console.ReadLine();
}

Dictionary<string, Int64> P2Cache = new();
void P2()
{
    Int64 cycleLength = -1;
    bool jumpMade = false;
    for (Int64 i = 0; i < 1_000_000_000; i++)
    {
        string strRep = "";
        if (!jumpMade) strRep = GetStringRep(mapP2);
        if (!jumpMade && P2Cache.TryGetValue(strRep, out Int64 previousOccurenceCycleIndex))
        {
            cycleLength = i - previousOccurenceCycleIndex;
            Int64 jumpTo = 1_000_000_000 - ((1_000_000_000 - i) % cycleLength);
            i = jumpTo - 1;
            jumpMade = true;
        }
        else
        {
            if (!jumpMade) P2Cache[strRep] = i;
            foreach (var roundRock in roundRocksP2.OrderBy(roundRock => roundRock.Y))
                roundRock.RollNorth(mapP2);
            foreach (var roundRock in roundRocksP2.OrderBy(roundRock => roundRock.X))
                roundRock.RollWest(mapP2);
            foreach (var roundRock in roundRocksP2.OrderByDescending(roundRock => roundRock.Y))
                roundRock.RollSouth(mapP2);
            foreach (var roundRock in roundRocksP2.OrderByDescending(roundRock => roundRock.X))
                roundRock.RollEast(mapP2);
        }
    }
    int result = roundRocksP2.Sum(roundRock => roundRock.CalculateLoad(mapHeight));
    Console.WriteLine(result);
    Console.ReadLine();
}

P1();
P2();

public enum CellType
{
    Rock = 'O',
    CubeRock = '#',
}

public class Cell
{
    public CellType CellType;
    public int X;
    public int Y;

    public Cell(char c, int x, int y)
    {
        CellType = (CellType)c;
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"{X},{Y},{(char)CellType}";
    }

    public void RollNorth(List<List<Cell?>> map)
    {
        int _y;
        for (_y = Y - 1; _y >= 0 && map[_y][X] is null; _y--) ;
        _y++;
        if (_y != Y)
        {
            map[Y][X] = null;
            this.Y = _y;
            map[Y][X] = this;
        }
    }

    public void RollSouth(List<List<Cell?>> map)
    {
        int _y;
        for (_y = Y + 1; _y < map.Count && map[_y][X] is null; _y++) ;
        _y--;
        if (_y != Y)
        {
            map[Y][X] = null;
            this.Y = _y;
            map[Y][X] = this;
        }
    }

    public void RollEast(List<List<Cell?>> map)
    {
        int _x;
        for (_x = X + 1; _x < map[Y].Count && map[Y][_x] is null; _x++) ;
        _x--;
        if (_x != X)
        {
            map[Y][X] = null;
            this.X = _x;
            map[Y][X] = this;
        }
    }

    public void RollWest(List<List<Cell?>> map)
    {
        int _x;
        for (_x = X - 1; _x >= 0 && map[Y][_x] is null; _x--) ;
        _x++;
        if (_x != X)
        {
            map[Y][X] = null;
            this.X = _x;
            map[Y][X] = this;
        }
    }

    public int CalculateLoad(int mapHeight)
    {
        return mapHeight - Y;
    }
}