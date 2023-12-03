using AdventOfCodeUtilities;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();

bool isNumber(char c)
{
    return (c >= '0' && c <= '9');
}

bool isCharacter(char c)
{
    return (c != '.' && (c < '0' || c > '9'));
}

int findNumber(string line, int x)
{
    int x1 = x;
    for (; x1 >= 0 && isNumber(line[x1]); x1--) ;
    x1++;
    int x2 = x1 + 1;
    for (; x2 < line.Length && isNumber(line[x2]); x2++) ;
    x2--;
    string sub = line[x1..(x2 + 1)];
    return int.Parse(sub);
}

void trial(HashSet<int> adjacentNumbers, int _y, int _x)
{
    if (_y >= 0 && _y < inputList.Count && _x >= 0 && _x < inputList[_y].Length && isNumber(inputList[_y][_x]))
        adjacentNumbers.Add(findNumber(inputList[_y], _x));
}

HashSet<int> search(int y, int x)
{
    HashSet<int> adjacentNumbers = new();
    // Could use a 3x3 for loop here, but this is clearer
    trial(adjacentNumbers, y - 1, x - 1);
    trial(adjacentNumbers, y - 1, x);
    trial(adjacentNumbers, y - 1, x + 1);
    trial(adjacentNumbers, y, x - 1);
    trial(adjacentNumbers, y, x + 1);
    trial(adjacentNumbers, y + 1, x - 1);
    trial(adjacentNumbers, y + 1, x);
    trial(adjacentNumbers, y + 1, x + 1);
    return adjacentNumbers;
}

void P1()
{
    int result = 0;

    for (int y = 0; y < inputList.Count; y++)
    {
        for (int x = 0; x < inputList[y].Length; x++)
        {
            if (isCharacter(inputList[y][x]))
            {
                var adjacentNumbers = search(y, x);
                result += adjacentNumbers.Sum();
            }
        }
    }
    Console.WriteLine(result);
    Console.ReadLine();
}

void P2()
{
    int result = 0;

    for (int y = 0; y < inputList.Count; y++)
    {
        for (int x = 0; x < inputList[y].Length; x++)
        {
            if (isCharacter(inputList[y][x]))
            {
                var adjacentNumbers = search(y, x);
                if (adjacentNumbers.Count == 2)
                    result += adjacentNumbers.Aggregate(1, (acc, val) => acc * val);
            }
        }
    }
    Console.WriteLine(result);
    Console.ReadLine();
}

P1();
P2();