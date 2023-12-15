using AdventOfCodeUtilities;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();
string input = inputList[0];
List<string> steps = input.Split(',').ToList();


int Hash(string s)
{
    int curVal = 0;
    foreach (char c in s)
    {
        curVal = ((curVal + c) * 17) % 256;
        /*
        curVal += c;
        curVal *= 17;
        curVal %= 256;
        */
    }
    return curVal;
}

void P1()
{
    int result = steps.Sum(Hash);
    Console.WriteLine(result);
    Console.ReadLine();
}

void P2()
{
    (List<string>, List<int>)[] boxes = new (List<string>, List<int>)[256];
    for (int i = 0; i < boxes.Length; i++)
        boxes[i] = (new(), new());
    foreach (var step in steps)
    {
        var split = step.Split(new char[] { '-', '=' }, StringSplitOptions.RemoveEmptyEntries);
        string label = split[0];
        int box = Hash(label);
        if (step.Contains('='))
        {
            int focalLength = int.Parse(split[1]);
            int index = boxes[box].Item1.IndexOf(label);
            if (index != -1)
                boxes[box].Item2[index] = focalLength;
            else
            {
                boxes[box].Item1.Add(label);
                boxes[box].Item2.Add(focalLength);
            }
        }
        else
        {
            int indexToRemove = boxes[box].Item1.IndexOf(label);
            if (indexToRemove != -1)
            {
                boxes[box].Item1.RemoveAt(indexToRemove);
                boxes[box].Item2.RemoveAt(indexToRemove);
            }
        }
    }

    Int64 sum = 0;
    for (int boxIndex = 0; boxIndex < boxes.Length; boxIndex++)
    {
        var box = boxes[boxIndex];
        for (int slotIndex = 0; slotIndex < box.Item1.Count; slotIndex++)
        {
            sum += (boxIndex + 1) * (slotIndex + 1) * box.Item2[slotIndex];
        }
    }

    Console.WriteLine(sum);
    Console.ReadLine();
}

P1();
P2();