using AdventOfCodeUtilities;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

List<string> inputList = AoC.GetInputLines();

List<Cell> cells = new();
List<List<Cell>> map = new();
int mapHeight = inputList.Count;
int mapWidth = inputList[0].Length;
for (int y = 0; y < mapHeight; y++)
{
    List<Cell> row = new();
    for (int x = 0; x < mapWidth; x++)
    {
        Cell newCell = new(x, y, inputList[y][x]);
        row.Add(newCell);
        cells.Add(newCell);
    }
    map.Add(row);
}

// P1
Node node = Node.GetOrCreate(null, 0, map[0][0], out bool createdNew);
Debug.Assert(createdNew);
List<Node> newNodesCreated = new() { node };
while (newNodesCreated.Any())
{
    Node newNode = newNodesCreated[0];
    newNodesCreated.RemoveAt(0);
    newNodesCreated.AddRange(newNode.ProcessNeighbours(map));
}
List<Node> nodesP1 = Node.NodeLibrary[false].Values.ToList();

// P2
node = Node.GetOrCreate(null, 0, map[0][0], out createdNew, true);
Debug.Assert(createdNew);
newNodesCreated = new() { node };
while (newNodesCreated.Any())
{
    Node newNode = newNodesCreated[0];
    newNodesCreated.RemoveAt(0);
    newNodesCreated.AddRange(newNode.ProcessNeighbours(map, true));
}
List<Node> nodesP2 = Node.NodeLibrary[true].Values.ToList();
//cells.ForEach(cell => cell.ProcessNeighbours(map));

void P1()
{
    Node startNode = map[0][0].NodesByFromDirectionAndDistanceInThisDirection[false][(null, 0)];
    startNode.Cost = 0;

    HashSet<Node> remainingNodesToVisit = new(nodesP1);
    PriorityQueue<Node, Int64> remainingNodes = new(nodesP1.Select(node => (node, node.Cost)));

    while (remainingNodesToVisit.Any())
    {
        Node currentNode = remainingNodes.Dequeue();

        foreach (var neighbourNode in currentNode.Neighbours.Where(neighbourNode => !neighbourNode.Visited))
        {
            Int64 newCost = currentNode.Cost + neighbourNode.Cell.HeatLoss;
            if (newCost < neighbourNode.Cost)
            {
                neighbourNode.Cost = newCost;
                remainingNodes.Enqueue(neighbourNode, neighbourNode.Cost);
            }
        }

        currentNode.Visited = true;
        remainingNodesToVisit.Remove(currentNode);
    }

    Int64 result = map[mapHeight - 1][mapWidth - 1].NodesByFromDirectionAndDistanceInThisDirection[false].Values.OrderBy(node => node.Cost).First().Cost;
    Console.WriteLine(result);
    Console.ReadLine();
}

void P2()
{
    Node startNode = map[0][0].NodesByFromDirectionAndDistanceInThisDirection[true][(null, 0)];
    startNode.Cost = 0;

    HashSet<Node> visitedNodes = new();
    HashSet<Node> remainingNodesToVisit = new(nodesP2);
    PriorityQueue<Node, Int64> remainingNodes = new(nodesP2.Select(node => (node, node.Cost)));

    while (remainingNodesToVisit.Any())
    {
        Node currentNode = remainingNodes.Dequeue();

        foreach (var neighbourNode in currentNode.Neighbours.Where(neighbourNode => !neighbourNode.Visited))
        {
            Int64 newCost = currentNode.Cost + neighbourNode.Cell.HeatLoss;
            if (newCost < neighbourNode.Cost)
            {
                neighbourNode.Cost = newCost;
                remainingNodes.Enqueue(neighbourNode, neighbourNode.Cost);
            }
        }

        currentNode.Visited = true;
        visitedNodes.Add(currentNode);
        remainingNodesToVisit.Remove(currentNode);
    }

    Int64 result = map[mapHeight - 1][mapWidth - 1].NodesByFromDirectionAndDistanceInThisDirection[true].Values.OrderBy(node => node.Cost).First().Cost;
    Console.WriteLine(result);
    Console.ReadLine();
}

P1();
P2();

public enum Direction
{
    North = 0,
    East = 1,
    South = 2,
    West = 3,
}

public class Node
{
    public bool Visited = false;
    public Int64 Cost = Int64.MaxValue;

    // Part 1 & 2
    public static Dictionary<bool, Dictionary<(Direction?, int, Cell), Node>> NodeLibrary = new()
    {
        { false, new() },
        { true, new() },
    };

    public Direction? FromDirection;
    public int DistanceInThisDirection;
    public Cell Cell;

    public List<Node> Neighbours = new();

    public Node(Direction? fromDirection, int distanceInThisDirection, Cell cell)
    {
        FromDirection = fromDirection;
        DistanceInThisDirection = distanceInThisDirection;
        Cell = cell;
    }

    public static Node GetOrCreate(Direction? fromDirection, int distanceInThisDirection, Cell cell, out bool createdNew, bool part2 = false)
    {
        var key = (fromDirection, distanceInThisDirection, cell);
        if (Node.NodeLibrary[part2].TryGetValue(key, out Node? node))
        {
            createdNew = false;
            return node!;
        }
        else
        {
            createdNew = true;
            Node newNode = new(fromDirection, distanceInThisDirection, cell);
            Node.NodeLibrary[part2][key] = newNode;
            return newNode;
        }
    }

    public static Direction InverseDirection(Direction d)
    {
        switch (d)
        {
            case Direction.North:
                return Direction.South;
            case Direction.South:
                return Direction.North;
            case Direction.East:
                return Direction.West;
            case Direction.West:
                return Direction.East;
            default:
                throw new Exception();
        }
    }

    public static (int, int) XYFromXYAndDirection(int x, int y, Direction d)
    {
        switch (d)
        {
            case Direction.North:
                return (x, y - 1);
            case Direction.South:
                return (x, y + 1);
            case Direction.East:
                return (x + 1, y);
            case Direction.West:
                return (x - 1, y);
            default:
                throw new Exception();
        }
    }

    public List<Node> ProcessNeighbours(List<List<Cell>> map, bool part2 = false)
    {
        List<Node> newNodesCreated = new();
        int minimumToTurn = part2 ? 4 : 0;
        int maximumStraight = part2 ? 10 : 3;
        if (FromDirection is null)
        {
            Debug.Assert(DistanceInThisDirection == 0);
            for (Direction toDirection = 0; (int)toDirection <= 3; toDirection += 1)
            {
                (int toX, int toY) = XYFromXYAndDirection(Cell.X, Cell.Y, toDirection);
                if (toY >= 0 && toY < map.Count && toX >= 0 && toX < map[toY].Count)
                {
                    Node newNeighbourNode = GetOrCreate(InverseDirection(toDirection), 1, map[toY][toX], out bool createdNew, part2);
                    Neighbours.Add(newNeighbourNode);
                    Cell.NodesByFromDirectionAndDistanceInThisDirection[part2][(FromDirection, DistanceInThisDirection)] = this;
                    if (createdNew)
                        newNodesCreated.Add(newNeighbourNode);
                }
            }
        }
        else // if (FromDirection is not null)
        {
            Direction straightDirection = InverseDirection(FromDirection.Value);

            Direction toDirection1 = (int)straightDirection > 0 ? straightDirection - 1 : (Direction)3;
            (int toX1, int toY1) = XYFromXYAndDirection(Cell.X, Cell.Y, toDirection1);
            if (toY1 >= 0 && toY1 < map.Count && toX1 >= 0 && toX1 < map[toY1].Count)
            {
                Cell cell = map[toY1][toX1];
                if (DistanceInThisDirection >= minimumToTurn)
                {
                    Node newNeighbourNode = GetOrCreate(InverseDirection(toDirection1), 1, cell, out bool createdNew, part2);
                    Neighbours.Add(newNeighbourNode);
                    Cell.NodesByFromDirectionAndDistanceInThisDirection[part2][(FromDirection, DistanceInThisDirection)] = this;
                    if (createdNew)
                        newNodesCreated.Add(newNeighbourNode);
                }
            }

            Direction toDirection2 = straightDirection;
            (int toX2, int toY2) = XYFromXYAndDirection(Cell.X, Cell.Y, toDirection2);
            if (toY2 >= 0 && toY2 < map.Count && toX2 >= 0 && toX2 < map[toY2].Count)
            {
                Cell cell = map[toY2][toX2];
                if (DistanceInThisDirection < maximumStraight)
                {
                    Node newNeighbourNode = GetOrCreate(InverseDirection(toDirection2), DistanceInThisDirection + 1, cell, out bool createdNew, part2);
                    Neighbours.Add(newNeighbourNode);
                    Cell.NodesByFromDirectionAndDistanceInThisDirection[part2][(FromDirection, DistanceInThisDirection)] = this;
                    if (createdNew)
                        newNodesCreated.Add(newNeighbourNode);
                }
            }

            Direction toDirection3 = (int)straightDirection < 3 ? straightDirection + 1 : (Direction)0;
            (int toX3, int toY3) = XYFromXYAndDirection(Cell.X, Cell.Y, toDirection3);
            if (toY3 >= 0 && toY3 < map.Count && toX3 >= 0 && toX3 < map[toY3].Count)
            {
                Cell cell = map[toY3][toX3];
                if (DistanceInThisDirection >= minimumToTurn)
                {
                    Node newNeighbourNode = GetOrCreate(InverseDirection(toDirection3), 1, cell, out bool createdNew, part2);
                    Neighbours.Add(newNeighbourNode);
                    Cell.NodesByFromDirectionAndDistanceInThisDirection[part2][(FromDirection, DistanceInThisDirection)] = this;
                    if (createdNew)
                        newNodesCreated.Add(newNeighbourNode);
                }
            }
        }
        return newNodesCreated;
    }

    public override string ToString()
    {
        return $"(Node C:{Cell} FD:{FromDirection} DITD:{DistanceInThisDirection}) Cost: {Cost}";
    }
}

public class Cell
{
    public int X;
    public int Y;
    public int HeatLoss;

    // Part 1 & 2
    public Dictionary<bool, Dictionary<(Direction?, int), Node>> NodesByFromDirectionAndDistanceInThisDirection = new()
    {
        { false, new() },
        { true, new() },
    };

    public Cell(int x, int y, char c)
    {
        this.X = x;
        this.Y = y;
        this.HeatLoss = int.Parse($"{c}");
    }

    public override string ToString()
    {
        return $"(Cell X:{X} Y:{Y} HL:{HeatLoss})";
    }
}