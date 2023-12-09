using AdventOfCodeUtilities;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();
List<Node> nodes = inputList.Skip(2).Select(l => new Node(l)).ToList();
nodes.ForEach(node => { node.Left = Node.Map[node.leftName]; node.Right = Node.Map[node.rightName]; });
char[] instructions = inputList[0].ToCharArray();

void P1()
{
    Node currentNode = Node.Map["AAA"];
    int instructionIndex = 0;
    int steps = 0;
    while (currentNode != Node.Map["ZZZ"])
    {
        switch (instructions[instructionIndex])
        {
            case 'R':
                currentNode = currentNode.Right!; break;
            case 'L':
                currentNode = currentNode.Left!; break;
        }

        instructionIndex = (instructionIndex + 1) % instructions.Length;
        steps++;
    }

    Console.WriteLine(steps);
    Console.ReadLine();
}

void P2()
{
    var currentNodes = nodes.Where(node => node.Name.EndsWith('A'));
    int[] cycleLengths = new int[currentNodes.Count()];
    int[] cyclePoints = new int[currentNodes.Count()];
    int[] cycleFinishes = new int[currentNodes.Count()];
    List<List<Node>> cycles = new();
    int j = 0;
    foreach (Node currentNode in currentNodes)
    {
        int instructionIndex = 0;
        int cycleLength = 0;
        Node cycleNode;
        cycleNode = currentNode;
        Dictionary<(int, Node), int> visitedNodes = new() { };
        (int, Node)? zPair = null;
        while (!visitedNodes.ContainsKey((instructionIndex, cycleNode)))
        {
            var pair = (instructionIndex, cycleNode);
            visitedNodes[pair] = cycleLength;

            if (cycleNode.Name.EndsWith('Z'))
                zPair = pair;

            switch (instructions[instructionIndex])
            {
                case 'R':
                    cycleNode = cycleNode.Right!; break;
                case 'L':
                    cycleNode = cycleNode.Left!; break;
            }

            instructionIndex = (instructionIndex + 1) % instructions.Length;
            cycleLength++;
        }
        cyclePoints[j] = visitedNodes[(instructionIndex, cycleNode)];
        cycleFinishes[j] = visitedNodes[zPair!.Value] - cyclePoints[j];
        cycleLengths[j] = cycleLength - cyclePoints[j];
        j++;
    }

    /*
    // If you assume 
    Int64 lcm = 1;
    for (int i = 0; i < cycleLengths.Length; i++)
    {
        lcm = AoC.LCM(lcm, cycleLengths[i]);
    }
    */

    Int64 steps = cycleLengths.Max();

    Int64 lcm;
    while (true)
    {
        if (((steps - cyclePoints[0]) % cycleLengths[0] == cycleFinishes[0]) && ((steps - cyclePoints[1]) % cycleLengths[1] == cycleFinishes[1]))
        {
            lcm = AoC.LCM(cycleLengths[0], cycleLengths[1]);
            break;
        }
        steps++;
    }
    for (int i = 2; i < cyclePoints.Length; i++)
    {
        while (true)
        {
            if (((steps - cyclePoints[i]) % cycleLengths[i] == cycleFinishes[i]))
            {
                lcm = AoC.LCM(lcm, cycleLengths[i]);
                break;
            }
            steps += lcm;
        }
    }

    Console.WriteLine(steps);
    Console.ReadLine();
}

P1();
P2();

public class Node
{
    public static Dictionary<string, Node> Map = new();

    public string Name;
    public string leftName;
    public string rightName;
    public Node? Left;
    public Node? Right;

    public Node(string l)
    {
        var split = l.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Name = split[0];
        leftName = split[2][1..^1];
        rightName = split[3][..^1];

        Map[Name] = this;
    }

    public override string ToString()
    {
        return Name;
    }
}