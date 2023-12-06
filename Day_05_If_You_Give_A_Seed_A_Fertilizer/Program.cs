using AdventOfCodeUtilities;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();

List<Int64> seeds = inputList[0].Split(' ', StringSplitOptions.RemoveEmptyEntries)[1..].Select(s => Int64.Parse(s)).ToList();
List<(Int64, Int64)> seedRanges = new();
for (int i = 0; i < seeds.Count; i += 2)
{
    seedRanges.Add((seeds[i], seeds[i + 1]));
}

// Naive dictionary based approach that didn't even work for Part 1 (Eric was mean on the range splitting puzzle this year)
/*
List<Dictionary<Int64, Int64>> maps = new List<Dictionary<Int64, Int64>>();
int _mapIndex = -1;
bool expectingNewMap = true;
for (int mapIndex = 2; mapIndex < inputList.Count; mapIndex++)
{
    string l = inputList[mapIndex];
    if (expectingNewMap && l != "")
    {
        expectingNewMap = false;
        _mapIndex++;
        maps.Add(new Dictionary<Int64, Int64>());
    }
    else
    {
        if (l == "")
        {
            expectingNewMap = true;
        }
        else
        {
            var split = l.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => Int64.Parse(s)).ToList();
            Int64 destRangeStart = split[0];
            Int64 _inputRangeStart = split[1];
            Int64 rangeLength = split[2];
            for (Int64 x = 0; x < rangeLength; x++)
            {
                maps[_mapIndex][_inputRangeStart + x] = destRangeStart + x;
            }
        }
    }
}
*/

// Construct map ranges
List<List<(Int64, Int64, Int64)>> maps = new();
int _mapIndex = -1;
bool expectingNewMap = true;
for (int i = 2; i < inputList.Count; i++)
{
    string l = inputList[i];
    if (expectingNewMap && l != "")
    {
        expectingNewMap = false;
        _mapIndex++;
        maps.Add(new());
    }
    else
    {
        if (l == "")
        {
            expectingNewMap = true;
        }
        else
        {
            var split = l.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => Int64.Parse(s)).ToList();
            Int64 destRangeStart = split[0];
            Int64 sourceRangeStart = split[1];
            Int64 rangeLength = split[2];
            maps[_mapIndex].Add((destRangeStart, sourceRangeStart, rangeLength));
        }
    }
}

Int64 MapSourceToDest(Int64 source, int mapIndex)
{
    Int64 dest = source;
    foreach (var region in maps[mapIndex])
    {
        (Int64 destRangeStart, Int64 sourceRangeStart, Int64 rangeLength) = region;
        Int64 diff = source - sourceRangeStart;
        if (diff >= 0 && diff < rangeLength)
        {
            dest = destRangeStart + diff;
            break;
        }
    }
    return dest;
}

void P1()
{
    Int64 result = seeds.Select(seed =>
    {
        Int64 source = seed;
        for (int mapIndex = 0; mapIndex < 7; mapIndex++)
        {
            Int64 mappedValue = MapSourceToDest(source, mapIndex);
            source = mappedValue;
        }
        return source;
    }).Min();
    Console.WriteLine(result);
    Console.ReadLine();
}

List<(Int64, Int64)> MapSourceRangeToDestRanges(Int64 _inputRangeStart, Int64 _inputRangeLength, int mapIndex)
{
    List<(Int64, Int64)> destRanges = new();

    List<(Int64, Int64)> inputRangesLeftToDealWith = new() { (_inputRangeStart, _inputRangeLength) };

    while (inputRangesLeftToDealWith.Any())
    {
        (Int64 inputRangeStart, Int64 inputRangeLength) = inputRangesLeftToDealWith.First();
        inputRangesLeftToDealWith.RemoveAt(0);

        Int64 inputRangeEnd = inputRangeStart + inputRangeLength - 1;
        bool handled = false;
        foreach (var region in maps[mapIndex])
        {
            (Int64 destRangeStart, Int64 sourceRangeStart, Int64 regionRangeLength) = region;
            Int64 destRangeEnd = destRangeStart + regionRangeLength - 1;
            Int64 sourceRangeEnd = sourceRangeStart + regionRangeLength - 1;

            if (inputRangeStart >= sourceRangeStart && inputRangeEnd <= sourceRangeEnd)
            {
                // Input contained entirely within one map region, easy to modify as its 1:f(1)
                Int64 outputRangeStart = (inputRangeStart - sourceRangeStart) + destRangeStart;
                Int64 outputRangeEnd = outputRangeStart + inputRangeLength - 1;
                Int64 outputRangeLength = outputRangeEnd - outputRangeStart + 1;
                destRanges.Add((outputRangeStart, outputRangeLength));

                handled = true;
                break;
            }
            else if (inputRangeStart < sourceRangeStart && inputRangeEnd > sourceRangeEnd)
            {
                // Handle middle region, easy to modify as it's 1:f(1)
                Int64 outputRangeStart = destRangeStart;
                Int64 outputRangeEnd = destRangeEnd;
                Int64 outputRangeLength = outputRangeEnd - outputRangeStart + 1;
                destRanges.Add((outputRangeStart, outputRangeLength));

                // Input overflows on both ends
                Int64 leftInputOverflowRangeStart = inputRangeStart;
                Int64 leftInputOverflowRangeEnd = sourceRangeStart - 1;
                Int64 leftInputOverflowRangeLength = leftInputOverflowRangeEnd - leftInputOverflowRangeStart + 1;
                Int64 rightInputOverflowRangeStart = sourceRangeEnd + 1;
                Int64 rightInputOverflowRangeEnd = inputRangeEnd;
                Int64 rightInputOverflowRangeLength = rightInputOverflowRangeEnd - rightInputOverflowRangeStart + 1;
                inputRangesLeftToDealWith.Add((leftInputOverflowRangeStart, leftInputOverflowRangeLength));
                inputRangesLeftToDealWith.Add((rightInputOverflowRangeStart, rightInputOverflowRangeLength));
                // Add these 2 ranges back to the list of reprocessing

                handled = true;
                break;
            }
            else if (inputRangeStart < sourceRangeStart && inputRangeEnd >= sourceRangeStart && inputRangeEnd <= sourceRangeEnd)
            {
                // Handle middle region, easy to modify as it's 1:f(1)
                Int64 outputRangeStart = destRangeStart;
                Int64 outputRangeEnd = destRangeStart + (inputRangeEnd - sourceRangeStart);
                Int64 outputRangeLength = outputRangeEnd - outputRangeStart + 1;
                destRanges.Add((outputRangeStart, outputRangeLength));

                // Input overflows on left
                Int64 leftInputOverflowRangeStart = inputRangeStart;
                Int64 leftInputOverflowRangeEnd = sourceRangeStart - 1;
                Int64 leftInputOverflowRangeLength = leftInputOverflowRangeEnd - leftInputOverflowRangeStart + 1;
                inputRangesLeftToDealWith.Add((leftInputOverflowRangeStart, leftInputOverflowRangeLength));
                // Add this range back to the list of reprocessing

                handled = true;
                break;
            }
            else if (inputRangeStart >= sourceRangeStart && inputRangeStart <= sourceRangeEnd && inputRangeEnd > sourceRangeEnd)
            {
                // Handle middle region, easy to modify as it's 1:f(1)
                Int64 outputRangeStart = destRangeStart + (inputRangeStart - sourceRangeStart);
                Int64 outputRangeEnd = destRangeEnd;
                Int64 outputRangeLength = outputRangeEnd - outputRangeStart + 1;
                destRanges.Add((outputRangeStart, outputRangeLength));

                // Input overflows on right
                Int64 rightInputOverflowRangeStart = sourceRangeEnd + 1;
                Int64 rightInputOverflowRangeEnd = inputRangeEnd;
                Int64 rightInputOverflowRangeLength = rightInputOverflowRangeEnd - rightInputOverflowRangeStart + 1;
                inputRangesLeftToDealWith.Add((rightInputOverflowRangeStart, rightInputOverflowRangeLength));
                // Add this range back to the list of reprocessing

                handled = true;
                break;
            }
            else
            {
                // Input range does not lie within this region, but doesn't mean it doesn't lie in other ones, so keep iterating
            }
        }
        if (!handled)
        {
            // If it completes the foreach loop with this still false, then the input range doesn't lie in any of the regions, so it's 1:1
            destRanges.Add((inputRangeStart, inputRangeLength));
        }
    }
    return destRanges;
}

void P2()
{
    List<(Int64, Int64)> sourceRanges = new(seedRanges);
    for (int mapIndex = 0; mapIndex < 7; mapIndex++)
    {
        List<(Int64, Int64)> destRanges = new();
        foreach (var sourceRange in sourceRanges)
        {
            (Int64 sourceRangeStart, Int64 sourceRangeLength) = sourceRange;
            List<(Int64, Int64)> newDestRanges = MapSourceRangeToDestRanges(sourceRangeStart, sourceRangeLength, mapIndex);
            destRanges.AddRange(newDestRanges);
        }
        sourceRanges = destRanges;
    }
    Int64 result = sourceRanges.MinBy(r => r.Item1).Item1;
    Console.WriteLine(result);
    Console.ReadLine();
}

P1();
P2();