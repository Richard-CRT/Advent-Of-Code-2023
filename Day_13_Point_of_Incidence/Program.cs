using AdventOfCodeUtilities;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();

List<List<List<CellType>>> patterns = new();
List<int> patternSummaries = new(); // used later to differentiate part 1 and 2

// Create the patterns
List<List<CellType>> _pattern = new();
for (int i = 0; i < inputList.Count; i++)
{
    string l = inputList[i];
    if (l == "")
    {
        patterns.Add(_pattern);
        _pattern = new();
    }
    else
    {
        _pattern.Add(l.Select(c => (CellType)c).ToList());
    }
}
patterns.Add(_pattern);

// Summarise the patterns in binary numbers representing each row and column
List<List<uint>> patternsRowStates = new();
foreach (List<List<CellType>> pattern in patterns)
{
    List<uint> patternRowStates = new();
    for (int rowIndex = 0; rowIndex < pattern.Count; rowIndex++)
    {
        int powerOfTwo = 0;
        uint state = 0;
        for (int x = 0; x < pattern[rowIndex].Count; x++)
        {
            if (pattern[rowIndex][x] == CellType.Rock)
            {
                state |= (uint)(1 << powerOfTwo);
            }
            powerOfTwo++;
        }
        patternRowStates.Add(state);
    }
    patternsRowStates.Add(patternRowStates);
}
List<List<uint>> patternsColumnStates = new();
foreach (List<List<CellType>> pattern in patterns)
{
    List<uint> patternColumnStates = new();
    for (int columnIndex = 0; columnIndex < pattern[0].Count; columnIndex++)
    {
        int powerOfTwo = 0;
        uint state = 0;
        for (int y = 0; y < pattern.Count; y++)
        {
            if (pattern[y][columnIndex] == CellType.Rock)
            {
                state |= (uint)(1 << powerOfTwo);
            }
            powerOfTwo++;
        }
        patternColumnStates.Add(state);
    }
    patternsColumnStates.Add(patternColumnStates);
}

int findAxisSymmetry(int axisIndex, List<uint> patternAxisStates)
{
    int startAxisIndex1 = axisIndex;
    int startAxisIndex2 = axisIndex + 1;
    bool symmetrical = true;
    int iterAxisIndex1 = startAxisIndex1;
    int iterAxisIndex2 = startAxisIndex2;
    while (iterAxisIndex1 >= 0 && iterAxisIndex2 <= patternAxisStates.Count - 1)
    {
        if (patternAxisStates[iterAxisIndex1] != patternAxisStates[iterAxisIndex2])
        {
            symmetrical = false;
            break;
        }
        iterAxisIndex1--;
        iterAxisIndex2++;
    }
    if (symmetrical)
    {
        return startAxisIndex2;
    }
    return -1;
}

void P1()
{
    Int64 result = 0;
    for (int patternIndex = 0; patternIndex < patterns.Count; patternIndex++)
    {
        bool foundSymmetry = false;

        var patternRowStates = patternsRowStates[patternIndex];
        // Check rows for symmetry
        for (int rowIndex = 0; rowIndex < patternRowStates.Count - 1; rowIndex++)
        {
            int mirrorRow = findAxisSymmetry(rowIndex, patternRowStates);
            if (mirrorRow != -1)
            {
                foundSymmetry = true;
                // Found symmetrical row
                int summary = 100 * mirrorRow;
                result += summary;
                patternSummaries.Add(summary);
            }
        }

        if (!foundSymmetry)
        {
            var patternColumnStates = patternsColumnStates[patternIndex];
            // Check Columns for symmetry
            for (int columnIndex = 0; columnIndex < patternColumnStates.Count - 1; columnIndex++)
            {
                int mirrorColumn = findAxisSymmetry(columnIndex, patternColumnStates);
                if (mirrorColumn != -1)
                {
                    foundSymmetry = true;
                    // Found symmetrical row
                    int summary = mirrorColumn;
                    result += summary;
                    patternSummaries.Add(summary);
                }
            }
        }
    }
    Console.WriteLine(result);
    Console.ReadLine();
}

void P2()
{
    Int64 result = 0;
    for (int patternIndex = 0; patternIndex < patterns.Count; patternIndex++)
    {
        bool foundSymmetry = false;
        // flip 1 bit
        for (int y = 0; y < patterns[patternIndex].Count; y++)
        {
            for (int x = 0; x < patterns[patternIndex][y].Count; x++)
            {
                // flip a given coordinate to see if it's the smudge
                List<uint> modifiedPatternRowStates = new(patternsRowStates[patternIndex]);
                modifiedPatternRowStates[y] ^= (uint)(1 << x);

                // Check rows for symmetry
                for (int rowIndex = 0; rowIndex < modifiedPatternRowStates.Count - 1; rowIndex++)
                {
                    int mirrorRow = findAxisSymmetry(rowIndex, modifiedPatternRowStates);
                    if (mirrorRow != -1)
                    {
                        // Check we haven't found the same symmetry as part 1
                        int summary = 100 * mirrorRow;
                        if (summary != patternSummaries[patternIndex])
                        {
                            // Found symmetrical row
                            foundSymmetry = true;
                            result += summary;
                        }
                    }
                }

                if (!foundSymmetry)
                {
                    // flip a given coordinate to see if it's the smudge
                    List<uint> modifiedPatternColumnStates = new(patternsColumnStates[patternIndex]);
                    modifiedPatternColumnStates[x] ^= (uint)(1 << y);

                    // Check Columns for symmetry
                    for (int columnIndex = 0; columnIndex < modifiedPatternColumnStates.Count - 1; columnIndex++)
                    {
                        int mirrorColumn = findAxisSymmetry(columnIndex, modifiedPatternColumnStates);
                        if (mirrorColumn != -1)
                        {
                            // Check we haven't found the same symmetry as part 1
                            int summary = mirrorColumn;
                            if (summary != patternSummaries[patternIndex])
                            {
                                // Found symmetrical row
                                foundSymmetry = true;
                                result += summary;
                            }
                        }
                    }
                }

                if (foundSymmetry)
                    break;
            }
            if (foundSymmetry)
                break;
        }

    }
    Console.WriteLine(result);
    Console.ReadLine();
}

P1();
P2();

public enum CellType
{
    Ash = '#',
    Rock = '.',
}