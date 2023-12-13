using AdventOfCodeUtilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();
List<(List<Type>, List<int>)> entries = inputList.Select(row =>
{
    var split = row.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    List<int> formatSplit = split[1].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToList();
    List<Type> inputSplit = split[0].Select(c => (Type)c).ToList();
    return (inputSplit, formatSplit);
}).ToList();

bool CheckIfFitFormat(List<Type> row, List<int> format)
{
    List<int> actualFormat = new();
    bool inBatch = false;
    int batchLength = 0;
    int batchIndex = -1;
    for (int i = 0; i < row.Count; i++)
    {
        if (row[i] == Type.Damaged)
        {
            if (!inBatch)
            {
                batchIndex++;
                inBatch = true;
                batchLength = 0;
            }
            batchLength++;
        }
        else if (row[i] == Type.Operational || row[i] == Type.Unknown)
        {
            if (inBatch)
            {
                inBatch = false;
                actualFormat.Add(batchLength);
            }
        }
        else
        {
            throw new Exception();
        }
    }
    if (inBatch)
    {
        actualFormat.Add(batchLength);
    }
    bool matches = true;
    if (actualFormat.Count == format.Count)
    {
        for (int i = 0; i < format.Count; i++)
        {
            if (format[i] != actualFormat[i])
            {
                matches = false;
                break;
            }
        }
    }
    else
        matches = false;
    return matches;
}

HashSet<string> existingEntries = new();
Int64 recurse(List<int> format, List<Type> springWithSubstitutions, int targetDepth, int depth = 1)
{
    if (depth == 1)
        existingEntries.Clear();
    List<Type> possibility = new(springWithSubstitutions);
    Int64 count = 0;
    for (int i = 0; i < springWithSubstitutions.Count; i++)
    {
        if (springWithSubstitutions[i] == Type.Unknown)
        {
            possibility[i] = Type.Damaged;
            string strRep = new string(possibility.Select(s => (char)s).ToArray());
            if (!existingEntries.Contains(strRep))
            {
                existingEntries.Add(strRep);
                if (depth == targetDepth)
                {
                    if (CheckIfFitFormat(possibility, format))
                    {
                        count++;
                    }
                }
                else
                {
                    Int64 recurseVal = recurse(format, possibility, targetDepth, depth + 1);
                    count += recurseVal;
                }
            }
            possibility[i] = Type.Unknown;
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
        (List<Type> springs, List<int> format) = entry;
        int numberOfDamaged = format.Sum();
        int numberOfKnownDamaged = springs.Count(s => s == Type.Damaged);
        int numberOfUnknownDamaged = numberOfDamaged - numberOfKnownDamaged;
        Int64 val;
        if (numberOfUnknownDamaged == 0)
            val = 1;
        else
            val = recurse(format, springs, numberOfUnknownDamaged);
        result += val;
        i++;
    }

    Console.WriteLine(result);
    Console.ReadLine();
}

void P2()
{
    int result = 0;
    Console.WriteLine(result);
    Console.ReadLine();
}

P1();
P2();

public enum Type
{
    Operational = '.',
    Damaged = '#',
    Unknown = '?'
}