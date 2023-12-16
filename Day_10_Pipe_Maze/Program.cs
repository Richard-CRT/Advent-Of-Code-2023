using AdventOfCodeUtilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();
int y = -1;
int x = -1;
List<List<Node>> map = inputList.Select(l => { y++; x = -1; return l.Select(c => { x++; return new Node(c, x, y); }).ToList(); }).ToList();
Node? startNode = null;
map.ForEach(row => row.ForEach(
    node =>
    {
        if (node.Type == Type.Unknown)
            startNode = node;
        node.FindConnections(map);
    }));
startNode!.DeduceType(map);
startNode.FindConnections(map);

void P1()
{
    // Quick loop through to determine which pieces are part of the main loop
    Node? currentNode = startNode;
    HashSet<Node> visitedNodes = new();
    int length = 1;
    while (currentNode is not null)
    {
        currentNode.PartOfLoop = true;
        visitedNodes.Add(currentNode);
        Node? nextNode = null;
        foreach (Node node in currentNode.Connections.Where(c => c is not null).Select(c => c!))
        {
            if (node is not null)
            {
                if (!visitedNodes.Contains(node))
                {
                    nextNode = node;
                    break;
                }
            }
        }
        if (nextNode is not null)
        {
            length++;
        }
        currentNode = nextNode;
    }

    Debug.Assert(length % 2 == 0);
    int result = length / 2;
    Console.WriteLine(result);
    Console.ReadLine();
}

void P2()
{
    // Loop through to find the nodes on left/right of the main loop
    Node? currentNode = startNode;
    HashSet<Node> visitedNodes = new();
    HashSet<Node> nodesOnLeftOfLoop = new();
    HashSet<Node> nodesOnRightOfLoop = new();
    while (currentNode is not null)
    {
        Direction direction = Direction.North; // default
        visitedNodes.Add(currentNode);
        Node? nextNode = null;
        for (int i = 0; i < 4; i++)
        {
            Node? node = currentNode.Connections[i];
            if (node is not null)
            {
                if (!visitedNodes.Contains(node))
                {
                    switch (i)
                    {
                        case 0: direction = Direction.North; break;
                        case 1: direction = Direction.East; break;
                        case 2: direction = Direction.South; break;
                        case 3: direction = Direction.West; break;
                    }
                    nextNode = node;
                    break;
                }
            }
        }
        if (nextNode is not null)
        {
            List<(int, int)> nodeOnLeftCoordinates = new();
            List<(int, int)> nodeOnRightCoordinates = new();
            switch (direction)
            {
                case Direction.North:
                    switch (currentNode.Type)
                    {
                        case Type.Vertical:
                            nodeOnLeftCoordinates.Add((currentNode.X - 1, currentNode.Y));
                            nodeOnRightCoordinates.Add((currentNode.X + 1, currentNode.Y));
                            break;
                        case Type.NorthEast:
                            nodeOnLeftCoordinates.Add((currentNode.X - 1, currentNode.Y));
                            nodeOnLeftCoordinates.Add((currentNode.X - 1, currentNode.Y + 1));
                            nodeOnLeftCoordinates.Add((currentNode.X, currentNode.Y + 1));
                            break;
                        case Type.NorthWest:
                            nodeOnRightCoordinates.Add((currentNode.X, currentNode.Y + 1));
                            nodeOnRightCoordinates.Add((currentNode.X + 1, currentNode.Y + 1));
                            nodeOnRightCoordinates.Add((currentNode.X + 1, currentNode.Y));
                            break;
                    }
                    break;
                case Direction.East:
                    switch (currentNode.Type)
                    {
                        case Type.Horizontal:
                            nodeOnLeftCoordinates.Add((currentNode.X, currentNode.Y - 1));
                            nodeOnRightCoordinates.Add((currentNode.X, currentNode.Y + 1));
                            break;
                        case Type.NorthEast:
                            nodeOnRightCoordinates.Add((currentNode.X - 1, currentNode.Y));
                            nodeOnRightCoordinates.Add((currentNode.X - 1, currentNode.Y + 1));
                            nodeOnRightCoordinates.Add((currentNode.X, currentNode.Y + 1));
                            break;
                        case Type.SouthEast:
                            nodeOnLeftCoordinates.Add((currentNode.X - 1, currentNode.Y));
                            nodeOnLeftCoordinates.Add((currentNode.X - 1, currentNode.Y - 1));
                            nodeOnLeftCoordinates.Add((currentNode.X, currentNode.Y - 1));
                            break;
                    }
                    break;
                case Direction.South:
                    switch (currentNode.Type)
                    {
                        case Type.Vertical:
                            nodeOnLeftCoordinates.Add((currentNode.X + 1, currentNode.Y));
                            nodeOnRightCoordinates.Add((currentNode.X - 1, currentNode.Y));
                            break;
                        case Type.SouthEast:
                            nodeOnRightCoordinates.Add((currentNode.X - 1, currentNode.Y));
                            nodeOnRightCoordinates.Add((currentNode.X - 1, currentNode.Y - 1));
                            nodeOnRightCoordinates.Add((currentNode.X, currentNode.Y - 1));
                            break;
                        case Type.SouthWest:
                            nodeOnLeftCoordinates.Add((currentNode.X, currentNode.Y - 1));
                            nodeOnLeftCoordinates.Add((currentNode.X + 1, currentNode.Y - 1));
                            nodeOnLeftCoordinates.Add((currentNode.X + 1, currentNode.Y));
                            break;
                    }
                    break;
                case Direction.West:
                    switch (currentNode.Type)
                    {
                        case Type.Horizontal:
                            nodeOnLeftCoordinates.Add((currentNode.X, currentNode.Y + 1));
                            nodeOnRightCoordinates.Add((currentNode.X, currentNode.Y - 1));
                            break;
                        case Type.NorthWest:
                            nodeOnLeftCoordinates.Add((currentNode.X + 1, currentNode.Y));
                            nodeOnLeftCoordinates.Add((currentNode.X + 1, currentNode.Y + 1));
                            nodeOnLeftCoordinates.Add((currentNode.X, currentNode.Y + 1));
                            break;
                        case Type.SouthWest:
                            nodeOnRightCoordinates.Add((currentNode.X + 1, currentNode.Y));
                            nodeOnRightCoordinates.Add((currentNode.X + 1, currentNode.Y - 1));
                            nodeOnRightCoordinates.Add((currentNode.X, currentNode.Y - 1));
                            break;
                    }
                    break;
            }

            foreach ((int xOfNodeOnLeft, int yOfNodeOnLeft) in nodeOnLeftCoordinates)
            {
                if (yOfNodeOnLeft >= 0 && yOfNodeOnLeft <= map.Count - 1

                    && xOfNodeOnLeft >= 0 && xOfNodeOnLeft <= map[yOfNodeOnLeft].Count - 1
                    && !map[yOfNodeOnLeft][xOfNodeOnLeft].PartOfLoop)
                {
                    map[yOfNodeOnLeft][xOfNodeOnLeft].LeftOfLoop = true;
                    nodesOnLeftOfLoop.Add(map[yOfNodeOnLeft][xOfNodeOnLeft]);
                }
            }

            foreach ((int xOfNodeOnRight, int yOfNodeOnRight) in nodeOnRightCoordinates)
            {
                if (yOfNodeOnRight >= 0 && yOfNodeOnRight <= map.Count - 1
                && xOfNodeOnRight >= 0 && xOfNodeOnRight <= map[yOfNodeOnRight].Count - 1
                && !map[yOfNodeOnRight][xOfNodeOnRight].PartOfLoop)
                {
                    map[yOfNodeOnRight][xOfNodeOnRight].RightOfLoop = true;
                    nodesOnRightOfLoop.Add(map[yOfNodeOnRight][xOfNodeOnRight]);
                }
            }
        }
        currentNode = nextNode;
    }

    // Relatively dirty method of determining which of Right/Left is the "inside" automatically
    bool outsideIsLeft = false;
    bool outsideIsRight = false;
    for (int y = 0; y < map.Count; y++)
    {
        outsideIsLeft = map[y][0].LeftOfLoop;
        if (outsideIsLeft) break;
        outsideIsRight = map[y][0].RightOfLoop;
        if (outsideIsRight) break;
        outsideIsLeft = map[y][map[y].Count - 1].LeftOfLoop;
        if (outsideIsLeft) break;
        outsideIsRight = map[y][map[y].Count - 1].RightOfLoop;
        if (outsideIsRight) break;
    }
    if (!outsideIsLeft && !outsideIsRight)
    {
        for (int x = 0; x < map[0].Count; x++)
        {
            outsideIsLeft = map[0][x].LeftOfLoop;
            if (outsideIsLeft) break;
            outsideIsRight = map[0][x].RightOfLoop;
            if (outsideIsRight) break;
            outsideIsLeft = map[map.Count - 1][x].LeftOfLoop;
            if (outsideIsLeft) break;
            outsideIsRight = map[map.Count - 1][x].RightOfLoop;
            if (outsideIsRight) break;
        }
    }

    HashSet<Node> totalNodesOnLeftOfLoop = new();
    if (outsideIsRight)
    {
        // Flood fill the nodes of the left of the loop
        while (nodesOnLeftOfLoop.Any())
        {
            foreach (Node node in nodesOnLeftOfLoop)
            {
                node.LeftOfLoop = true;
                totalNodesOnLeftOfLoop.Add(node);
            }
            HashSet<Node> newNodesOnLeftOfLoop = new();
            foreach (Node nodeOnLeftOfLoop in nodesOnLeftOfLoop)
            {
                var someNewNodesOnLeftOfLoop = nodeOnLeftOfLoop.GetNonLeftAdjacentNonLoopNodes(map);
                foreach (Node newNodeOnLeftOfLoop in someNewNodesOnLeftOfLoop)
                    newNodesOnLeftOfLoop.Add(newNodeOnLeftOfLoop);
            }
            nodesOnLeftOfLoop = newNodesOnLeftOfLoop;
        }
    }

    HashSet<Node> totalNodesOnRightOfLoop = new();
    if (outsideIsLeft)
    {
        // Flood fill the nodes on the right of the loop
        while (nodesOnRightOfLoop.Any())
        {
            foreach (Node node in nodesOnRightOfLoop)
            {
                node.RightOfLoop = true;
                totalNodesOnRightOfLoop.Add(node);
            }
            HashSet<Node> newNodesOnRightOfLoop = new();
            foreach (Node nodeOnRightOfLoop in nodesOnRightOfLoop)
            {
                var someNewNodesOnRightOfLoop = nodeOnRightOfLoop.GetNonRightAdjacentNonLoopNodes(map);
                foreach (Node newNodeOnRightOfLoop in someNewNodesOnRightOfLoop)
                    newNodesOnRightOfLoop.Add(newNodeOnRightOfLoop);
            }
            nodesOnRightOfLoop = newNodesOnRightOfLoop;
        }
    }

    //PrintLoop();

    if (outsideIsRight)
    {
        Console.WriteLine(totalNodesOnLeftOfLoop.Count());
        Console.ReadLine();
    }
    else if (outsideIsLeft)
    {
        Console.WriteLine(totalNodesOnRightOfLoop.Count());
        Console.ReadLine();
    }
}

#pragma warning disable CS8321
void PrintLoop()
{
    for (y = 0; y < map.Count; y++)
    {
        for (x = 0; x < map[y].Count; x++)
        {
            if (map[y][x].PartOfLoop)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($"{Node.typeToPrintMaps[map[y][x].Type]}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                if (map[y][x].RightOfLoop)
                    Console.Write("R");
                else if (map[y][x].LeftOfLoop)
                    Console.Write("L");
                else
                    Console.Write("░");
            }
        }
        Console.WriteLine();
    }
}

P1();
P2();

public enum Direction
{
    East,
    West,
    South,
    North
}

public enum Type
{
    Vertical = '|',
    Horizontal = '-',
    NorthEast = 'L',
    NorthWest = 'J',
    SouthWest = '7',
    SouthEast = 'F',
    Ground = '.',
    Unknown = 'S'
}

public class Node
{
    public static Dictionary<char, Type> typeMap = new()
    {
    { '|', Type.Vertical },
    { '-', Type.Horizontal },
    { 'L', Type.NorthEast },
    { 'J', Type.NorthWest },
    { '7', Type.SouthWest },
    { 'F', Type.SouthEast },
    { '.', Type.Ground },
    { 'S', Type.Unknown }
    };
    public static Dictionary<Type, char> typeToPrintMaps = new()
    {
    { Type.Vertical, '│' },
    { Type.Horizontal,'─' },
    { Type.NorthEast, '└'},
    { Type.NorthWest, '┘' },
    { Type.SouthWest,'┐' },
    { Type.SouthEast,'┌' },
    };

    public int X;
    public int Y;
    public Type Type;
    // 0 up
    // 1 right
    // 2 down
    // 3 left
    public Node?[] Connections = new Node?[4];
    public bool PartOfLoop = false;
    public bool LeftOfLoop = false;
    public bool RightOfLoop = false;


    public Node(char c, int x, int y)
    {
        Type = typeMap[c];
        X = x;
        Y = y;
    }

    public void FindConnections(List<List<Node>> map)
    {
        switch (Type)
        {
            case Type.Vertical:
                if (Y > 0)
                    Connections[0] = map[Y - 1][X];
                if (Y < map.Count - 1)
                    Connections[2] = map[Y + 1][X];
                break;
            case Type.Horizontal:
                if (X > 0)
                    Connections[3] = map[Y][X - 1];
                if (X < map[Y].Count - 1)
                    Connections[1] = map[Y][X + 1];
                break;
            case Type.NorthEast:
                if (Y > 0)
                    Connections[0] = map[Y - 1][X];
                if (X < map[Y].Count - 1)
                    Connections[1] = map[Y][X + 1];
                break;
            case Type.SouthEast:
                if (X < map[Y].Count - 1)
                    Connections[1] = map[Y][X + 1];
                if (Y < map.Count - 1)
                    Connections[2] = map[Y + 1][X];
                break;
            case Type.SouthWest:
                if (Y < map.Count - 1)
                    Connections[2] = map[Y + 1][X];
                if (X > 0)
                    Connections[3] = map[Y][X - 1];
                break;
            case Type.NorthWest:
                if (Y > 0)
                    Connections[0] = map[Y - 1][X];
                if (X > 0)
                    Connections[3] = map[Y][X - 1];
                break;
        }
    }

    public Type DeduceType(List<List<Node>> map)
    {
        HashSet<Type> possibleTypes = Enum.GetValues(typeof(Type)).Cast<Type>().ToHashSet();
        if (Y > 0)
        {
            if (map[Y - 1][X].Connections[2] == this)
            {
                possibleTypes.IntersectWith(new HashSet<Type>() { Type.Vertical, Type.NorthEast, Type.NorthWest });
            }
        }
        if (Y < map.Count - 1)
        {
            if (map[Y + 1][X].Connections[0] == this)
            {
                possibleTypes.IntersectWith(new HashSet<Type>() { Type.Vertical, Type.SouthEast, Type.SouthWest });
            }
        }
        if (X > 0)
        {
            if (map[Y][X - 1].Connections[1] == this)
            {
                possibleTypes.IntersectWith(new HashSet<Type>() { Type.Horizontal, Type.SouthWest, Type.NorthWest });
            }
        }
        if (X < map[Y].Count - 1)
        {
            if (map[Y][X + 1].Connections[3] == this)
            {
                possibleTypes.IntersectWith(new HashSet<Type>() { Type.Horizontal, Type.SouthEast, Type.NorthEast });
            }
        }

        Debug.Assert(possibleTypes.Count == 1);
        Type = possibleTypes.First();
        return Type;
    }

    /*
    // Stack overflow so not used, but would theoretically work
    public int GetLengthOfLoopRecursively(HashSet<Node> visitedNodes, int i = 0)
    {
        visitedNodes.Add(this);
        i++;
        foreach (Node? connection in Connections)
        {
            if (connection is not null && !visitedNodes.Contains(connection))
            {
                return connection.GetLengthOfLoopRecursively(visitedNodes, i);
            }
        }
        return i;
    }
    */

    public IEnumerable<Node> GetNonLeftAdjacentNonLoopNodes(List<List<Node>> map)
    {
        var nodes = GetAdjacentNonLoopNodes(map);
        return nodes.Where(node => node.LeftOfLoop == false);
    }

    public IEnumerable<Node> GetNonRightAdjacentNonLoopNodes(List<List<Node>> map)
    {
        var nodes = GetAdjacentNonLoopNodes(map);
        return nodes.Where(node => node.RightOfLoop == false);
    }

    public HashSet<Node> GetAdjacentNonLoopNodes(List<List<Node>> map)
    {
        HashSet<Node> adjacentNodes = new();
        for (int _y = -1; _y <= 1; _y++)
        {
            for (int _x = -1; _x <= 1; _x++)
            {
                if (Math.Abs(_x) + Math.Abs(_y) == 1)
                {
                    int trialY = Y + _y;
                    int trialX = X + _x;
                    if (trialY >= 0 && trialY <= map.Count - 1 && trialX >= 0 && trialX <= map[Y].Count - 1 && !map[trialY][trialX].PartOfLoop)
                    {
                        adjacentNodes.Add(map[trialY][trialX]);
                    }
                }
            }
        }
        return adjacentNodes;
    }
}