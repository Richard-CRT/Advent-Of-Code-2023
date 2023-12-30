using AdventOfCodeUtilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();
Dictionary<(int, int), char> map = new();
List<List<Int64>> mapPart2 = new();
(int, int) startCoord = (0, 0);
int mapWidth = inputList[0].Length;
int mapHeight = inputList.Count;
for (int y = 0; y < inputList.Count; y++)
{
    mapPart2.Add(new());
    for (int x = 0; x < inputList[y].Length; x++)
    {
        char c = inputList[y][x];
        if (c == 'S')
        {
            map[(x, y)] = '.';
            mapPart2[y].Add(-1);
            startCoord = (x, y);
        }
        else
        {
            map[(x, y)] = c;
            if (c == '.')
            {
                mapPart2[y].Add(-1);
            }
            else
            {
                mapPart2[y].Add(-2);
            }
        }
    }
}

void PrintMap(List<List<Int64>> mapToPrint, (int, int)? mapStartCoord = null, int? limit = null)
{
    mapStartCoord ??= startCoord;
    limit ??= mapWidth;

    for (int y = 0; y < inputList.Count; y++)
    {
        for (int x = 0; x < inputList[y].Length; x++)
        {
            if (mapToPrint[y][x] == -2)
                Console.Write("#");
            else if (mapToPrint[y][x] == -1)
                Console.Write(".");
            else
            {
                if (mapToPrint[y][x] <= limit)
                {
                    if (mapToPrint[y][x] % 2 == 0)
                        Console.Write("E");
                    else
                        Console.Write("O");
                }
                else
                    Console.Write(" ");
            }
        }
        Console.WriteLine();
    }

    Int64 maxStepCount = mapToPrint.Max(row => row.Max());
    double fraction = (double)1 / maxStepCount;
    for (int y = 0; y < inputList.Count; y++)
    {
        for (int x = 0; x < inputList[y].Length; x++)
        {
            if (mapStartCoord == (x, y))
                Console.Write("╬");
            else
            {
                if (mapToPrint[y][x] == -2)
                    Console.Write("♦");
                else
                {
                    if (mapToPrint[y][x] == -1)
                        Console.Write(".");
                    else
                    {
                        double val = mapToPrint[y][x] * fraction;
                        if (val > 0.75)
                            Console.Write("░");
                        else if (val > 0.5)
                            Console.Write("▒");
                        else if (val > 0.25)
                            Console.Write("▓");
                        else
                            Console.Write('█');
                    }
                }
            }
        }
        Console.WriteLine();
    }

    for (int y = 0; y < inputList.Count; y++)
    {
        for (int x = 0; x < inputList[y].Length; x++)
        {
            if (mapStartCoord == (x, y))
                Console.Write("  ╬ ");
            else
            {
                if (mapToPrint[y][x] == -2)
                    Console.Write(" ██ ");
                else
                {
                    if (mapToPrint[y][x] == -1)
                        Console.Write("  . ");
                    else
                        Console.Write(mapToPrint[y][x].ToString().PadLeft(3, ' ') + " ");
                }
            }
        }
        Console.WriteLine();
    }
}

#pragma warning disable CS8321
void PrintStuff(Dictionary<(int, int), List<List<Int64>>> gridMapToPrint)
{
    foreach (var kvp in gridMapToPrint)
    {
        (int, int) gridCoord = kvp.Key;
        (int gridX, int gridY) = gridCoord;
        Console.WriteLine($"Grid X:{gridX} Y:{gridY}");

        List<List<Int64>> mapToPrint = kvp.Value;

        PrintMap(mapToPrint);
    }
}

void P1()
{
    // BFS but without excluding the cells already visited
    HashSet<(int, int)> newCoords = new() { startCoord };
    HashSet<(int, int)> gardenPlots = new();
    for (int i = 0; i < 64; i++)
    {
        HashSet<(int, int)> newNewCoords = new();
        foreach ((int x, int y) in newCoords)
        {
            foreach ((int _x, int _y) in new List<(int, int)>() { (-1, 0), (0, -1), (1, 0), (0, 1) })
            {
                (int, int) trial = (x + _x, y + _y);
                if (map.TryGetValue(trial, out char cellType) && cellType == '.')
                {
                    newNewCoords.Add(trial);
                    gardenPlots.Add(trial);
                }
            }
        }
        newCoords = newNewCoords;
    }

    Console.WriteLine(newCoords.Count);
    Console.ReadLine();
}

List<List<Int64>> Copy(List<List<Int64>> toCopy)
{
    return toCopy.Select(row => new List<Int64>(row)).ToList();
}


void P2General() // General solution that 'will' solve any input, but do it with massive memory use and pretty slowly
{
    var cleanMap = Copy(mapPart2);

    // BFS but without excluding the cells already visited
    Dictionary<(int, int), List<List<Int64>>> gridMap = new()
    {
        { (0,0), Copy(cleanMap) }
    };

    HashSet<((int, int), (int, int))> newCoords = new() { ((0, 0), startCoord) };
    //bool newFound = true;
    int stepLimit = 5000;
    for (int i = 1; i <= stepLimit; i++)
    {
        //newFound = false;
        bool even = i % 2 == 0;
        HashSet<((int, int), (int, int))> newNewCoords = new();
        foreach (((int gridX, int gridY), (int x, int y)) in newCoords)
        {
            foreach ((int _x, int _y) in new List<(int, int)>() { (-1, 0), (0, -1), (1, 0), (0, 1) })
            {
                int trialX = x + _x;
                int trialY = y + _y;
                int trialGridX = gridX;
                int trialGridY = gridY;
                if (trialX < 0)
                {
                    trialGridX--;
                    trialX += mapWidth;
                }
                else if (trialX > mapWidth - 1)
                {
                    trialGridX++;
                    trialX -= mapWidth;
                }
                if (trialY < 0)
                {
                    trialGridY--;
                    trialY += mapHeight;
                }
                else if (trialY > mapHeight - 1)
                {
                    trialGridY++;
                    trialY -= mapHeight;
                }

                (int, int) gridTrial = (trialGridX, trialGridY);
                List<List<Int64>> grid;
                if (!gridMap.TryGetValue((gridTrial), out grid!))
                {
                    grid = Copy(cleanMap);
                    gridMap[gridTrial] = grid;
                }

                if (grid[trialY][trialX] == -1)
                {
                    //newFound = true;
                    grid[trialY][trialX] = i;
                    (int, int) trial = (trialX, trialY);
                    newNewCoords.Add((gridTrial, trial));
                }
            }
        }
        newCoords = newNewCoords;
    }

    //PrintStuff(gridMap);
    Console.WriteLine(gridMap.Sum(kvp => kvp.Value.Sum(row => row.Count(steps => steps >= 0 && steps % 2 == stepLimit % 2))));
    Console.ReadLine();
}

#if false // Terrible attempt at code to do the solution that did end up working more generally - do not advise reading
void P2()
{
    var cleanMap = Copy(map_part2);
    HashSet<(int, int)> startCoords = new() { startCoord };
    /*
    for (int y = 0; y < mapHeight; y++)
    {
        startCoords.Add((0, y));
        startCoords.Add((mapWidth - 1, y));
    }
    for (int x = 0; x < mapWidth; x++)
    {
        startCoords.Add((x, 0));
        startCoords.Add((x, mapHeight - 1));
    }
    */
    startCoords.Add(((int)(mapWidth / 2), 0));
    startCoords.Add(((int)(mapWidth / 2), mapHeight - 1));
    startCoords.Add((0, (int)(mapHeight / 2)));
    startCoords.Add((mapWidth - 1, (int)(mapHeight / 2)));
    Dictionary<(int, int), List<List<Int64>>> mapsByStartCoord = new();
    foreach (var startCoord in startCoords)
        mapsByStartCoord[startCoord] = Copy(cleanMap);

    foreach (var kvp in mapsByStartCoord)
    {
        var startCoord = kvp.Key;
        (int startCoordX, int startCoordY) = startCoord;
        var map = kvp.Value;

        HashSet<(int, int)> newCoords = new();
        newCoords.Add(startCoord);
        map[startCoordY][startCoordX] = 1;

        // BFS but without excluding the cells already visited
        bool newFound = true;
        for (int i = 2; newFound; i++)
        {
            newFound = false;
            bool even = i % 2 == 0;
            HashSet<(int, int)> newNewCoords = new();
            foreach ((int x, int y) in newCoords)
            {
                foreach ((int _x, int _y) in new List<(int, int)>() { (-1, 0), (0, -1), (1, 0), (0, 1) })
                {
                    int trialX = x + _x;
                    int trialY = y + _y;
                    (int, int) trial = (trialX, trialY);
                    if (trialX >= 0 && trialX < mapWidth && trialY >= 0 && trialY < mapHeight && map[trialY][trialX] == -1)
                    {
                        newFound = true;
                        map[trialY][trialX] = i;
                        newNewCoords.Add(trial);
                    }
                }
            }
            newCoords = newNewCoords;
        }

        //PrintMap(map, startCoord);
        //Console.ReadLine();
    }

    Dictionary<(int, int), (Int64, (int, int))> gridMap = new() { { (0, 0), (0, startCoord) } };
    Queue<(int, int)> gridCoordsToProcess = new();
    gridCoordsToProcess.Enqueue((0, 0));
    while (gridCoordsToProcess.Any())
    {
        (int, int) gridCoordToProcess = gridCoordsToProcess.Dequeue();
        (int gridCoordToProcessX, int gridCoordToProcessY) = gridCoordToProcess;
        (Int64 offset, (int, int) startCoord) = gridMap[gridCoordToProcess];
        (int startCoordX, int startCoordY) = startCoord;
        List<List<Int64>> map = mapsByStartCoord[startCoord];
        //Console.WriteLine($"Grid X:{gridCoordToProcessX} Y:{gridCoordToProcessY}");
        //PrintMap(map);
        //Console.ReadLine();

        // Studying input reveals shortest path to edge will always be on the centre coordinate of the edge

        foreach (((int _x, int _y), (int endCoordX, int endCoordY)) in new List<((int,int), (int,int))>() {
            ((-1, 0),(0, (int)(mapHeight / 2))),
            ((0, -1),((int)(mapWidth/2), 0)),
            ((1, 0),(mapWidth - 1, (int)(mapHeight / 2))),
            ((0, 1),((int)(mapWidth / 2), mapHeight - 1))
        })
        {
            int trialX = gridCoordToProcessX + _x;
            int trialY = gridCoordToProcessY + _y;
            (int, int) trial = (trialX, trialY);
            if (!gridMap.ContainsKey(trial))
            {
                Int64 nextOffset = offset + map[endCoordY][endCoordX];
                if (nextOffset < 26501365)
                {
                    int trialStartCoordX = endCoordX + _x;
                    int trialStartCoordY = endCoordY + _y;
                    if (trialStartCoordX < 0)
                        trialStartCoordX += mapWidth;
                    else if (trialStartCoordX >= mapWidth)
                        trialStartCoordX -= mapWidth;
                    if (trialStartCoordY < 0)
                        trialStartCoordY += mapHeight;
                    else if (trialStartCoordY >= mapHeight)
                        trialStartCoordY -= mapHeight;
                    gridMap[trial] = (nextOffset, (trialStartCoordX, trialStartCoordY));
                    gridCoordsToProcess.Enqueue(trial);
                }
            }
        }
    }

    const int stepNumber = 10;

    //Console.WriteLine("Calculating numbers");
    //Console.WriteLine(gridMap.Sum(kvp => kvp.Value.Sum(row => row.Where(steps => steps >= 0 && steps % 2 == 0).Count())));
    Console.ReadLine();
}
#endif

void P2()
{
    // Studying input reveals shortest path to edge will always be on the centre coordinate of the edge

    // 65 steps to get to the edge
    const int steps = 26501365;
    //const int steps = 49; // My test input pattern
    double cellsInOneDirection = (steps - (int)(mapWidth / 2)) / (double)mapWidth;
    int completeCellsInOneDirection = (int)cellsInOneDirection;
    // Manual inspection tells us cellsInOneDirection is already an integer, meaning
    // going all the way in one direction gets us to the far edge of a grid cell
    // and also means it reaches the centre of the cell with 65 left (i.e. the same as the start cell)


    var cleanMap = Copy(mapPart2);
    HashSet<(int, int)> startCoords = new() { startCoord };
    startCoords.Add(((int)(mapWidth / 2), 0));
    startCoords.Add(((int)(mapWidth / 2), mapHeight - 1));
    startCoords.Add((0, (int)(mapHeight / 2)));
    startCoords.Add((mapWidth - 1, (int)(mapHeight / 2)));
    startCoords.Add((mapWidth - 1, mapHeight - 1));
    startCoords.Add((0,0));
    startCoords.Add((mapWidth - 1, 0));
    startCoords.Add((0, mapHeight - 1));
    Dictionary<(int, int), List<List<Int64>>> mapsByStartCoord = new();
    foreach (var iterStartCoord in startCoords)
        mapsByStartCoord[iterStartCoord] = Copy(cleanMap);
    foreach (var kvp in mapsByStartCoord)
    {
        var iterStartCoord = kvp.Key;
        var map = kvp.Value;

        (int iterStartCoordX, int iterStartCoordY) = iterStartCoord;
        if (iterStartCoord == startCoord)
            map[iterStartCoordY][iterStartCoordX] = 2;
        else
            map[iterStartCoordY][iterStartCoordX] = 1;
        HashSet<(int, int)> newCoords = new() { iterStartCoord };

        bool newFound = true;
        int i;
        if (iterStartCoord == startCoord)
        {
            map[iterStartCoordY][iterStartCoordX] = 2;
            i = 1;
        }
        else
        {
            map[iterStartCoordY][iterStartCoordX] = 1;
            i = 2;
        }
        for (; newFound; i++)
        {
            newFound = false;
            bool even = i % 2 == 0;
            HashSet<(int, int)> newNewCoords = new();
            foreach ((int x, int y) in newCoords)
            {
                foreach ((int _x, int _y) in new List<(int, int)>() { (-1, 0), (0, -1), (1, 0), (0, 1) })
                {
                    int trialX = x + _x;
                    int trialY = y + _y;
                    (int, int) trial = (trialX, trialY);
                    if (trialX >= 0 && trialX < mapWidth && trialY >= 0 && trialY < mapHeight && map[trialY][trialX] == -1)
                    {
                        newFound = true;
                        map[trialY][trialX] = i;
                        newNewCoords.Add(trial);
                    }
                }
            }
            newCoords = newNewCoords;
        }
    }

    // Good luck understanding this in a week...
    List<List<Int64>> CMap = mapsByStartCoord[startCoord];
    List<List<Int64>> BMap = mapsByStartCoord[((int)(mapWidth / 2), mapHeight - 1)];
    List<List<Int64>> TMap = mapsByStartCoord[((int)(mapWidth / 2), 0)];
    List<List<Int64>> RMap = mapsByStartCoord[(mapWidth - 1, (int)(mapHeight / 2))];
    List<List<Int64>> LMap = mapsByStartCoord[(0, (int)(mapHeight / 2))];
    List<List<Int64>> BRMap = mapsByStartCoord[(mapWidth - 1,mapHeight - 1)];
    List<List<Int64>> BLMap = mapsByStartCoord[(0,mapHeight - 1)];
    List<List<Int64>> TRMap = mapsByStartCoord[(mapWidth - 1,0)];
    List<List<Int64>> TLMap = mapsByStartCoord[(0,0)];

    //PrintMap(CMap, default, mapWidth);
    //PrintMap(BRMap, default, ((int)(mapWidth / 2)));
    //PrintMap(BRMap, default, (mapWidth + (int)(mapWidth / 2)));
    //PrintMap(BLMap, default, ((int)(mapWidth / 2)));
    //PrintMap(BLMap, default, (mapWidth + (int)(mapWidth / 2)));
    //PrintMap(TRMap, default, (mapWidth + (int)(mapWidth / 2)));
    //PrintMap(TRMap, default, ((int)(mapWidth / 2)));
    //PrintMap(TLMap, default, (mapWidth + (int)(mapWidth / 2)));
    //PrintMap(TLMap, default, ((int)(mapWidth / 2)));

    // Magic
    Int64 numberOfEvenGardenPlotsAccessibleWithMapDimCMap = CMap.Sum(row => row.Where(steps => steps >= 0 && steps <= mapWidth && steps % 2 == 0).Count());
    Int64 numberOfOddGardenPlotsAccessibleWithMapDimCMap = CMap.Sum(row => row.Where(steps => steps >= 0 && steps <= mapWidth && steps % 2 == 1).Count());
    Int64 numberOfEvenGardenPlotsAccessibleWithMapDimRMap = RMap.Sum(row => row.Where(steps => steps >= 0 && steps <= mapWidth && steps % 2 == 0).Count());
    Int64 numberOfOddGardenPlotsAccessibleWithMapDimRMap = RMap.Sum(row => row.Where(steps => steps >= 0 && steps <= mapWidth && steps % 2 == 1).Count());
    Int64 numberOfEvenGardenPlotsAccessibleWithMapDimLMap = LMap.Sum(row => row.Where(steps => steps >= 0 && steps <= mapWidth && steps % 2 == 0).Count());
    Int64 numberOfOddGardenPlotsAccessibleWithMapDimLMap = LMap.Sum(row => row.Where(steps => steps >= 0 && steps <= mapWidth && steps % 2 == 1).Count());
    Int64 numberOfEvenGardenPlotsAccessibleWithMapDimBMap = BMap.Sum(row => row.Where(steps => steps >= 0 && steps <= mapWidth && steps % 2 == 0).Count());
    Int64 numberOfOddGardenPlotsAccessibleWithMapDimBMap = BMap.Sum(row => row.Where(steps => steps >= 0 && steps <= mapWidth && steps % 2 == 1).Count());
    Int64 numberOfEvenGardenPlotsAccessibleWithMapDimTMap = TMap.Sum(row => row.Where(steps => steps >= 0 && steps <= mapWidth && steps % 2 == 0).Count());
    Int64 numberOfOddGardenPlotsAccessibleWithMapDimTMap = TMap.Sum(row => row.Where(steps => steps >= 0 && steps <= mapWidth && steps % 2 == 1).Count());
    // These ones require even/odd inversion as starting in corner flips it
    // START
    Int64 numberOfOddGardenPlotsAccessibleWithMapDimBRMap = BRMap.Sum(row => row.Where(steps => steps >= 0 && steps <= (mapWidth + (int)(mapWidth/2)) && steps % 2 == 0).Count());
    Int64 numberOfEvenGardenPlotsAccessibleWithMapDimBRMap = BRMap.Sum(row => row.Where(steps => steps >= 0 && steps <= (mapWidth + (int)(mapWidth / 2)) && steps % 2 == 1).Count());
    Int64 numberOfOddGardenPlotsAccessibleWithMapDimBLMap = BLMap.Sum(row => row.Where(steps => steps >= 0 && steps <= (mapWidth + (int)(mapWidth / 2)) && steps % 2 == 0).Count());
    Int64 numberOfEvenGardenPlotsAccessibleWithMapDimBLMap = BLMap.Sum(row => row.Where(steps => steps >= 0 && steps <= (mapWidth + (int)(mapWidth / 2)) && steps % 2 == 1).Count());
    Int64 numberOfOddGardenPlotsAccessibleWithMapDimTRMap = TRMap.Sum(row => row.Where(steps => steps >= 0 && steps <= (mapWidth + (int)(mapWidth / 2)) && steps % 2 == 0).Count());
    Int64 numberOfEvenGardenPlotsAccessibleWithMapDimTRMap = TRMap.Sum(row => row.Where(steps => steps >= 0 && steps <= (mapWidth + (int)(mapWidth / 2)) && steps % 2 == 1).Count());
    Int64 numberOfOddGardenPlotsAccessibleWithMapDimTLMap = TLMap.Sum(row => row.Where(steps => steps >= 0 && steps <= (mapWidth + (int)(mapWidth / 2)) && steps % 2 == 0).Count());
    Int64 numberOfEvenGardenPlotsAccessibleWithMapDimTLMap = TLMap.Sum(row => row.Where(steps => steps >= 0 && steps <= (mapWidth + (int)(mapWidth / 2)) && steps % 2 == 1).Count());

    Int64 numberOfOddGardenPlotsAccessibleWithMapDimBRMapComp = BRMap.Sum(row => row.Where(steps => steps >= 0 && steps <= ((int)(mapWidth / 2)) && steps % 2 == 0).Count());
    Int64 numberOfEvenGardenPlotsAccessibleWithMapDimBRMapComp = BRMap.Sum(row => row.Where(steps => steps >= 0 && steps <= ((int)(mapWidth / 2)) && steps % 2 == 1).Count());
    Int64 numberOfOddGardenPlotsAccessibleWithMapDimBLMapComp = BLMap.Sum(row => row.Where(steps => steps >= 0 && steps <= ((int)(mapWidth / 2)) && steps % 2 == 0).Count());
    Int64 numberOfEvenGardenPlotsAccessibleWithMapDimBLMapComp = BLMap.Sum(row => row.Where(steps => steps >= 0 && steps <= ((int)(mapWidth / 2)) && steps % 2 == 1).Count());
    Int64 numberOfOddGardenPlotsAccessibleWithMapDimTRMapComp = TRMap.Sum(row => row.Where(steps => steps >= 0 && steps <= ((int)(mapWidth / 2)) && steps % 2 == 0).Count());
    Int64 numberOfEvenGardenPlotsAccessibleWithMapDimTRMapComp = TRMap.Sum(row => row.Where(steps => steps >= 0 && steps <= ((int)(mapWidth / 2)) && steps % 2 == 1).Count());
    Int64 numberOfOddGardenPlotsAccessibleWithMapDimTLMapComp = TLMap.Sum(row => row.Where(steps => steps >= 0 && steps <= ((int)(mapWidth / 2)) && steps % 2 == 0).Count());
    Int64 numberOfEvenGardenPlotsAccessibleWithMapDimTLMapComp = TLMap.Sum(row => row.Where(steps => steps >= 0 && steps <= ((int)(mapWidth / 2)) && steps % 2 == 1).Count());
    // END

    // A closed formula definitely exists for this iteration, but couldn't be bothered
    Int64 numberOfEvenCompleteCells = 0;
    Int64 numberOfOddCompleteCells = 0;
    bool iterEven = true;
    numberOfOddCompleteCells++;
    for (int i = 1; i <= completeCellsInOneDirection - 1; i++, iterEven = !iterEven)
    {
        if (iterEven)
            numberOfEvenCompleteCells += 4 * i;
        else
            numberOfOddCompleteCells += 4 * i;
    }

    Int64 numberOfEvenGardenPlotsAccessibleCompleteCells = numberOfEvenCompleteCells * numberOfEvenGardenPlotsAccessibleWithMapDimCMap;
    Int64 numberOfOddGardenPlotsAccessibleCompleteCells = numberOfOddCompleteCells * numberOfOddGardenPlotsAccessibleWithMapDimCMap;
    Int64 numberOfGardenPlotsAccessibleCompleteCells = numberOfEvenGardenPlotsAccessibleCompleteCells + numberOfOddGardenPlotsAccessibleCompleteCells;

    Int64 numberOfEvenGardenPlotsAccessibleTips = numberOfEvenGardenPlotsAccessibleWithMapDimRMap + numberOfEvenGardenPlotsAccessibleWithMapDimLMap + numberOfEvenGardenPlotsAccessibleWithMapDimBMap + numberOfEvenGardenPlotsAccessibleWithMapDimTMap;
    Int64 numberOfOddGardenPlotsAccessibleTips = numberOfOddGardenPlotsAccessibleWithMapDimRMap + numberOfOddGardenPlotsAccessibleWithMapDimLMap + numberOfOddGardenPlotsAccessibleWithMapDimBMap + numberOfOddGardenPlotsAccessibleWithMapDimTMap;
    Int64 numberOfGardenPlotsAccessibleTips = steps % 2 == 0 ? numberOfEvenGardenPlotsAccessibleTips : numberOfOddGardenPlotsAccessibleTips;

    Int64 numberOfEvenGardenPlotsAccessibleDiagonalsMain = (completeCellsInOneDirection - 1) * (numberOfEvenGardenPlotsAccessibleWithMapDimBRMap + numberOfEvenGardenPlotsAccessibleWithMapDimBLMap + numberOfEvenGardenPlotsAccessibleWithMapDimTRMap + numberOfEvenGardenPlotsAccessibleWithMapDimTLMap);
    Int64 numberOfEvenGardenPlotsAccessibleDiagonalComplements = (completeCellsInOneDirection) * (numberOfEvenGardenPlotsAccessibleWithMapDimBRMapComp + numberOfEvenGardenPlotsAccessibleWithMapDimBLMapComp + numberOfEvenGardenPlotsAccessibleWithMapDimTRMapComp + numberOfEvenGardenPlotsAccessibleWithMapDimTLMapComp);
    Int64 numberOfOddGardenPlotsAccessibleDiagonalsMain = (completeCellsInOneDirection - 1) * (numberOfOddGardenPlotsAccessibleWithMapDimBRMap + numberOfOddGardenPlotsAccessibleWithMapDimBLMap + numberOfOddGardenPlotsAccessibleWithMapDimTRMap + numberOfOddGardenPlotsAccessibleWithMapDimTLMap);
    Int64 numberOfOddGardenPlotsAccessibleDiagonalsComplements = (completeCellsInOneDirection) * (numberOfOddGardenPlotsAccessibleWithMapDimBRMapComp + numberOfOddGardenPlotsAccessibleWithMapDimBLMapComp + numberOfOddGardenPlotsAccessibleWithMapDimTRMapComp + numberOfOddGardenPlotsAccessibleWithMapDimTLMapComp);
    Int64 numberOfGardenPlotsAccessibleDiagonalsMain = steps % 2 == 0 ? numberOfEvenGardenPlotsAccessibleDiagonalsMain : numberOfOddGardenPlotsAccessibleDiagonalsMain;
    // Note these ones are opposite parity to the diagonal mains
    Int64 numberOfGardenPlotsAccessibleDiagonalsComplements = steps % 2 == 0 ? numberOfOddGardenPlotsAccessibleDiagonalsComplements : numberOfEvenGardenPlotsAccessibleDiagonalComplements;
    Int64 numberOfGardenPlotsAccessibleDiagonals = numberOfGardenPlotsAccessibleDiagonalsMain + numberOfGardenPlotsAccessibleDiagonalsComplements;

    Int64 numberOfGardenPlotsAccessible = numberOfGardenPlotsAccessibleCompleteCells + numberOfGardenPlotsAccessibleTips + numberOfGardenPlotsAccessibleDiagonals;

    Console.WriteLine(numberOfGardenPlotsAccessible);
    Console.ReadLine();
}

P1();
P2();

public class Plot
{
    public int X;
    public int Y;
    private bool visited;
    public bool Visited
    {
        get { return visited; }
        private set { visited = value; }
    }
    private Int64 visitedOnStep = -1;
    public Int64 VisitedOnStep
    {
        get
        {
            return visitedOnStep;
        }
        set
        {
            Visited = true;
            visitedOnStep = value;
        }
    }

    private bool odd;
    public bool Odd
    {
        get { return odd; }
        set { even = !value; odd = value; }
    }
    private bool even;
    public bool Even
    {
        get { return even; }
        set { odd = !value; even = value; }
    }

    public Plot(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return ".";
    }
    public string ToEvenOddString()
    {
        return VisitedOnStep != -1 ? Even ? "E" : "O" : ".";
    }
}