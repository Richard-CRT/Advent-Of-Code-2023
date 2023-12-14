using AdventOfCodeUtilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();
List<(int, int, UInt128, UInt128, List<int>)> entries = inputList.Select(row =>
{
    var split = row.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    List<int> formatSplit = split[1].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToList();
    UInt128 springs = 0;
    UInt128 unknownBitwise = 0;
    for (int i = 0; i < split[0].Length; i++)
    {
        if (split[0][i] == '#')
            springs |= ((UInt128)1 << i);
        if (split[0][i] == '?')
            unknownBitwise |= ((UInt128)1 << i);
    }
    return (split[0].Length, split[0].Count(c => c == '#'), springs, unknownBitwise, formatSplit);
}).ToList();
List<(int, int, UInt128, UInt128, List<int>)> unfoldedEntries = new();

string BitwiseToString(int rowLength, UInt128 springWithSubstitutions, UInt128 unknownBitwise)
{
    string s = "";
    for (int i = 0; i < rowLength; i++)
    {
        UInt128 powerOf2 = ((UInt128)1 << i);
        if ((unknownBitwise & powerOf2) != 0)
        {
            s += '?';
        }
        else if ((springWithSubstitutions & powerOf2) != 0)
        {
            s += '#';
        }
        else
        {
            s += '.';
        }
    }
    return s;
}

int CheckIfFitFormat(UInt128 possibility, int rowLength, List<int> format)
{
    //string s = BitwiseToString(rowLength, possibility, 0);
    List<int> actualFormat = new();
    bool inBatch = false;
    int batchLength = 0;
    int batchIndex = -1;
    for (int i = 0; i < rowLength; i++)
    {
        UInt128 powerOf2 = ((UInt128)1 << i);
        if ((possibility & powerOf2) != 0)
        {
            if (!inBatch)
            {
                batchIndex++;
                inBatch = true;
                batchLength = 0;
            }
            batchLength++;
        }
        else
        {
            if (inBatch)
            {
                inBatch = false;
                actualFormat.Add(batchLength);
            }
        }
    }
    if (inBatch)
    {
        actualFormat.Add(batchLength);
    }
    int matches = 0;
    if (actualFormat.Count > format.Count)
        return -1;
    for (int i = 0; i < actualFormat.Count; i++)
    {
        if (format[i] == actualFormat[i])
        {
            matches++;
        }
        else
        {
            matches = -1;
            break;
        }
    }
    return matches;
}

State GetStateOfBitwise(UInt128 springs, UInt128 unknownBitwise, int index)
{
    UInt128 powerOf2 = ((UInt128)1 << index);
    if ((unknownBitwise & powerOf2) != 0)
        return State.Unknown;
    else if ((springs & powerOf2) != 0)
        return State.Damaged;
    else
        return State.Operational;
}

HashSet<(UInt128, UInt128)> existingEntries = new();
Int64 recurse(List<int> format, int rowLength, UInt128 springWithSubstitutions, UInt128 unknownBitwise, int targetDepth, int depth = 1)
{
    if (depth == 1)
        existingEntries.Clear();
    Int64 count = 0;
    for (int i = 0; i < rowLength; i++)
    {
        UInt128 powerOf2 = ((UInt128)1 << i);
        if ((unknownBitwise & powerOf2) != 0)
        {
            UInt128 possibility = springWithSubstitutions | powerOf2;
            UInt128 possibilityUnknownBitwise = unknownBitwise & (~powerOf2);
            (UInt128, UInt128) cacheKey = (possibility, possibilityUnknownBitwise);
            /*
            Console.WriteLine(BitwiseToString(rowLength, springWithSubstitutions, unknownBitwise));
            Console.WriteLine("->");
            Console.WriteLine(BitwiseToString(rowLength, possibility, possibilityUnknownBitwise));
            Console.WriteLine($"{Convert.ToString((Int64)unknownBitwise, 2).PadLeft(rowLength, '0')}");
            Console.WriteLine($"{Convert.ToString((Int64)possibilityUnknownBitwise, 2).PadLeft(rowLength, '0')}");
            Console.ReadLine();
            */
            if (!existingEntries.Contains(cacheKey))
            {
                existingEntries.Add(cacheKey);
                if (depth == targetDepth)
                {
                    if (CheckIfFitFormat(possibility, rowLength, format) == format.Count)
                    {
                        count++;
                    }
                }
                else
                {
                    Int64 recurseVal = recurse(format, rowLength, possibility, possibilityUnknownBitwise, targetDepth, depth + 1);
                    count += recurseVal;
                }
            }
        }
    }
    return count;
}

Int64 recurseP2(List<int> format, int rowLength, UInt128 springWithSubstitutions, UInt128 unknownBitwise, int depth = 0)
{
    if (format.Count == 0)
    {
        if (springWithSubstitutions == 0)
        {
            Console.WriteLine($"{"".PadLeft(depth * 4, ' ')}MATCH");
            return 1;
        }
        else
        {
            return 0;
        }
    }
    else
    {
        if (unknownBitwise == 0)
        {
            if (CheckIfFitFormat(springWithSubstitutions, rowLength, format) == format.Count)
            {
                Console.WriteLine($"{"".PadLeft(depth * 4, ' ')}MATCH");
                return 1;
            }
            else
                return 0;
        }
    }
    Int64 count = 0;
    int batchLengthWeAreLookingFor = format[0];
    // looking for a straight run of unknowns/damaged machines batchLengthWeAreLookingFor long
    // if it's damaged machines it has to be the right length
    // if it's unknowns we have to permutate
    int i;
    for (i = 0; GetStateOfBitwise(springWithSubstitutions, unknownBitwise, i) == State.Operational; i++) ;
    // i is the first index that isn't just operational
    int j;
    List<int> indexOfUnknowns = new();
    for (j = i; j < rowLength; j++)
    {
        State state = GetStateOfBitwise(springWithSubstitutions, unknownBitwise, j);
        if (state == State.Operational)
            break;
        if (state == State.Unknown)
            indexOfUnknowns.Add(j - i);
    }
    int straightRunLength = j - i;
    // j is the first index after the straight run
    // we need to permutate this straight run, (including with all .)
    UInt128 straightRunMask = (((UInt128)1 << j) - 1) - (((UInt128)1 << i) - 1);
    UInt128 straightRun = (springWithSubstitutions & straightRunMask) >> i;
    UInt128 straightRunUnknownBitwise = (unknownBitwise & straightRunMask) >> i;
    Console.WriteLine($"{"".PadLeft(depth * 4, ' ')}Total: {BitwiseToString(rowLength, springWithSubstitutions, unknownBitwise)}");
    Console.WriteLine($"{"".PadLeft(depth * 4, ' ')}Straight run mask: {Convert.ToString((Int64)straightRunMask, 2).PadLeft(rowLength, '0')}");
    Console.WriteLine($"{"".PadLeft(depth * 4, ' ')}Straight run: {BitwiseToString(straightRunLength, straightRun, straightRunUnknownBitwise)}");
    for (UInt128 permutation = 0; permutation < ((UInt128)1 << indexOfUnknowns.Count); permutation++)
    {
        UInt128 possibility = straightRun;
        for (int n = 0; n < indexOfUnknowns.Count; n++)
        {
            possibility |= ((permutation & ((UInt128)1 << n)) >> n) << indexOfUnknowns[n];
        }
        int howManyFormatBlocksDoesItMatch = CheckIfFitFormat(possibility, straightRunLength, format);
        if (howManyFormatBlocksDoesItMatch >= 0)
        {
            Console.WriteLine($"{"".PadLeft(depth * 4, ' ')}Permutation: {Convert.ToString((Int64)permutation, 2).PadLeft(indexOfUnknowns.Count, '0')}");
            //Console.WriteLine($"Possibility: {Convert.ToString((Int64)possibility, 2).PadLeft(straightRunLength, '0')}");
            Console.WriteLine($"{"".PadLeft(depth * 4, ' ')}Possibility: {BitwiseToString(straightRunLength, possibility, 0)}");
            Console.WriteLine($"{"".PadLeft(depth * 4, ' ')}Matches # format blocks: {howManyFormatBlocksDoesItMatch}");

            // strip the format part it matches and pass to child recursion
            UInt128 leftOver = (springWithSubstitutions & ~straightRunMask) >> j;
            UInt128 leftOverUnknownBitwise = (unknownBitwise & ~straightRunMask) >> j;
            int leftOverLength = rowLength - j;
            List<int> leftOverFormat = format.GetRange(howManyFormatBlocksDoesItMatch, format.Count - howManyFormatBlocksDoesItMatch);
            Console.WriteLine($"{"".PadLeft(depth * 4, ' ')}Left over: {BitwiseToString(leftOverLength, leftOver, leftOverUnknownBitwise)}");
            count += recurseP2(leftOverFormat, leftOverLength, leftOver, leftOverUnknownBitwise, depth + 1);
        }
        else
        {
            // incompatible with format, invalid
        }
    }
    return count;
}

void P1()
{
    Int64 result = 0;

    int i = 0;
    foreach (var entry in entries)
    {
        Console.WriteLine(i);
        (int rowLength, int numberOfKnownDamaged, UInt128 springs, UInt128 unknownBitwise, List<int> format) = entry;
        int numberOfDamaged = format.Sum();
        int numberOfUnknownDamaged = numberOfDamaged - numberOfKnownDamaged;
        Int64 val;
        if (numberOfUnknownDamaged == 0)
            val = 1;
        else
            val = recurseP2(format, rowLength, springs, unknownBitwise);
            //val = recurse(format, rowLength, springs, unknownBitwise, numberOfUnknownDamaged);
        //Console.WriteLine(val);
        result += val;
        i++;
    }

    Console.WriteLine(result);
    Console.ReadLine();
}

void P2()
{
    Int64 result = 0;

    int i = 0;
    foreach (var entry in unfoldedEntries)
    {
        Console.WriteLine(i);
        (int rowLength, int numberOfKnownDamaged, UInt128 springs, UInt128 unknownBitwise, List<int> format) = entry;
        int numberOfDamaged = format.Sum();
        int numberOfUnknownDamaged = numberOfDamaged - numberOfKnownDamaged;
        Int64 val;
        if (numberOfUnknownDamaged == 0)
            val = 1;
        else
            val = recurse(format, rowLength, springs, unknownBitwise, numberOfUnknownDamaged);
        //Console.WriteLine(val);
        result += val;
        i++;
    }

    Console.WriteLine(result);
    Console.ReadLine();
}

P1();

foreach (var entry in entries)
{
    (int rowLength, int numberOfKnownDamaged, UInt128 springs, UInt128 unknownBitwise, List<int> format) = entry;

    int newRowLength = (rowLength * 5) + (1 * 4);
    int newNumberOfKnownDamaged = 5 * numberOfKnownDamaged;
    List<int> newFormat = new(format);
    UInt128 newSprings = springs;
    UInt128 newUnknownBitwise = unknownBitwise;
    Console.WriteLine($"####");
    Console.WriteLine($"{Convert.ToString((Int64)unknownBitwise, 2).PadLeft(newRowLength, '0')}");
    Console.WriteLine($"{Convert.ToString((Int64)springs, 2).PadLeft(newRowLength, '0')}");
    for (int i = 1; i <= 4; i++)
    {
        newUnknownBitwise |= ((UInt128)1 << (i * (rowLength + 1)) - 1);
        newSprings |= (springs << i * (rowLength + 1));
        newUnknownBitwise |= (unknownBitwise << i * (rowLength + 1));
        //springs.AddRange(originalSprings);
        newFormat.AddRange(format);
    }
    Console.WriteLine($"Unknown bitwise: {Convert.ToString((Int64)newUnknownBitwise, 2).PadLeft(newRowLength, '0')}");
    Console.WriteLine($"Spring row:      {Convert.ToString((Int64)newSprings, 2).PadLeft(newRowLength, '0')}");
    Console.WriteLine(BitwiseToString(newRowLength, newSprings, newUnknownBitwise));
    Console.WriteLine(string.Join(',', newFormat));

    unfoldedEntries.Add((newRowLength, newNumberOfKnownDamaged, newSprings, newUnknownBitwise, newFormat));
}

P2();

public enum State
{
    Operational = '.',
    Damaged = '#',
    Unknown = '?'
}