using AdventOfCodeUtilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();

List<(string, string)> connections = new();
Dictionary<string, HashSet<(int, string)>> connectionsByComponentName = new();
inputList.ForEach(l =>
{
    var split = l.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    string primary = split[0][..^1];
    string[] secondaries = split[1..];
    foreach (string secondary in secondaries)
    {
        int comp = primary.CompareTo(secondary);
        (string, string) connection;
        if (comp == -1)
            connection = (primary, secondary);
        else if (comp == 1)
            connection = (secondary, primary);
        else
            throw new Exception();

        HashSet<(int, string)>? hs;
        if (!connectionsByComponentName.TryGetValue(primary, out hs))
        {
            hs = new();
            connectionsByComponentName[primary] = hs;
        }
        hs.Add((connections.Count, secondary));
        if (!connectionsByComponentName.TryGetValue(secondary, out hs))
        {
            hs = new();
            connectionsByComponentName[secondary] = hs;
        }
        hs.Add((connections.Count, primary));

        connections.Add(connection);
    }
});
List<string> componentNames = new(connectionsByComponentName.Keys);

void AssignGroups(HashSet<int> connectionIndexesToExclude)
{
    int groupIndex = 0;
    Dictionary<string, int> groupByComponentName = new();
    foreach (string componentName in componentNames)
    {
        if (!groupByComponentName.ContainsKey(componentName))
        {
            Queue<string> searchComponentNames = new();
            HashSet<string> searchedComponentNames = new() { componentName };
            searchComponentNames.Enqueue(componentName);
            while (searchComponentNames.Any())
            {
                string searchComponentName = searchComponentNames.Dequeue();
                groupByComponentName[searchComponentName] = groupIndex;

                foreach ((int connectionIndex, string connectedComponentName) in connectionsByComponentName[searchComponentName])
                {
                    if (!connectionIndexesToExclude.Contains(connectionIndex))
                    {
                        if (!groupByComponentName.ContainsKey(connectedComponentName) && !searchedComponentNames.Contains(connectedComponentName))
                        {
                            //Console.WriteLine($"    {connectionIndex} {connectedComponentName}");
                            searchComponentNames.Enqueue(connectedComponentName);
                            searchedComponentNames.Add(connectedComponentName);
                        }
                    }
                }
            }
            groupIndex++;
        }
    }

    Debug.Assert(groupByComponentName.Keys.Count == componentNames.Count);
    Debug.Assert(groupIndex == 2);

    int sizeGroup0 = groupByComponentName.Count(kvp => kvp.Value == 0);
    int sizeGroup1 = groupByComponentName.Count(kvp => kvp.Value == 1);
    int result = sizeGroup0 * sizeGroup1;
    Console.WriteLine(result);
    Console.ReadLine();
    return;
}

void P1Naive()
{
    for (int i = 0; i < connections.Count; i++)
    {
        for (int j = i + 1; j < connections.Count; j++)
        {
            for (int k = j + 1; k < connections.Count; k++)
            {
                AssignGroups(new HashSet<int>() { i, j, k });
            }
        }
    }
}


void P1()
{
    int[] occurencesByConnectionIndex = new int[connections.Count];
    for (int i = 0; i < componentNames.Count; i++)
    {
        string componentNameA = componentNames[i];

        int componentFurthestFromAPathLength = int.MinValue;
        List<List<int>> componentFurthestFromAPaths = new();
        //string componentFurthestFromA;

        Queue<(int, List<int>, string)> searchComponentNames = new();
        HashSet<string> searchedComponentNames = new() { componentNameA };
        searchComponentNames.Enqueue((0, new(), componentNameA));
        while (searchComponentNames.Any())
        {
            (int pathLength, List<int> path, string searchComponentName) = searchComponentNames.Dequeue();

            if (pathLength > componentFurthestFromAPathLength)
            {
                componentFurthestFromAPathLength = pathLength;
                componentFurthestFromAPaths = new() { path };
                //componentFurthestFromA = searchComponentName;
            }
            else if (pathLength == componentFurthestFromAPathLength)
            {
                componentFurthestFromAPaths.Add(path);
            }

            foreach ((int connectionIndex, string connectedComponentName) in connectionsByComponentName[searchComponentName])
            {
                if (!searchedComponentNames.Contains(connectedComponentName))
                {
                    List<int> copyOfPath = (new(path));
                    copyOfPath.Add(connectionIndex);
                    searchComponentNames.Enqueue((pathLength + 1, copyOfPath, connectedComponentName));
                    searchedComponentNames.Add(connectedComponentName);
                }
            }
        }

        componentFurthestFromAPaths.ForEach(componentFurthestFromAPath => componentFurthestFromAPath.ForEach(connectionIndex => occurencesByConnectionIndex[connectionIndex]++));
    }
    var connectionIndexAndOccurences = occurencesByConnectionIndex.Select((int occurrences, int index) => (index, occurrences));
    var connectionIndexAndOccurencesOrdered = connectionIndexAndOccurences.OrderByDescending(pair => pair.occurrences);
    var threeMostUsedConnectionIndexesOrdered = connectionIndexAndOccurencesOrdered.Take(3).Select(pair => pair.index);
    //.ForEach(pair => Console.WriteLine($"{pair.Item1} {connections[pair.Item1]} {pair.Item2}"));
    HashSet<int> connectionIndexesToExclude = threeMostUsedConnectionIndexesOrdered.ToHashSet();
    //connectionIndexesToExclude.ForEach(index => Console.WriteLine($"{index} {connections[index]}"));

    AssignGroups(connectionIndexesToExclude);
}

P1();
