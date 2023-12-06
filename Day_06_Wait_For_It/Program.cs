using AdventOfCodeUtilities;
using System.Diagnostics;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();

List<int> times = inputList[0].Split(' ', StringSplitOptions.RemoveEmptyEntries)[1..].Select(s => int.Parse(s)).ToList();
List<int> distances = inputList[1].Split(' ', StringSplitOptions.RemoveEmptyEntries)[1..].Select(s => int.Parse(s)).ToList();

int timeP2 = int.Parse(string.Join("", inputList[0].Split(' ', StringSplitOptions.RemoveEmptyEntries)[1..]));
Int64 distanceP2 = Int64.Parse(string.Join("", inputList[1].Split(' ', StringSplitOptions.RemoveEmptyEntries)[1..].Select(s => int.Parse(s)).ToList()));

Int64 numWays(Int64 time, Int64 distance)
{
    Int64 numWays = 0;

    // time_held * time_remaining = distance
    // time_held * (constant - time_held) = distance
    // -time_held^2 + constant * time_held - distance = 0
    // time_held^2 - constant * tile_held + distance = 0
    // time_held = constant +/- sqrt(constant^2-(4*1*distance)) / (2*1)
    Int64 constant = time;
    double discriminant = Math.Sqrt((constant * constant) - (4 * 1 * distance));
    double timeHeld1 = (time + discriminant) / (2 * 1);
    double timeHeld2 = (time - discriminant) / (2 * 1);
    // result is quadratic
    // for given distance, there are two solutions of time_held

    double timeHeldMax = (double)Math.Max(timeHeld1, timeHeld2);
    double timeHeldMin = (double)Math.Min(timeHeld1, timeHeld2);

    Int64 timeHeldMax_int = timeHeldMax % 1 == 0 ? (Int64)Math.Round(timeHeldMax - 1) : (Int64)Math.Floor(timeHeldMax);
    Int64 timeHeldMin_int = timeHeldMin % 1 == 0 ? (Int64)Math.Round(timeHeldMin + 1) : (Int64)Math.Ceiling(timeHeldMin);

    numWays = timeHeldMax_int - timeHeldMin_int + 1;
    return numWays;
}

void P1()
{
    Int64 result = 1;
    for (int race = 0; race < times.Count; race++)
    {
        /*
        // Quick and dirty to get a solution quickly
        int numWays = 0;
        for (int charge_time = 0; charge_time < times[race]; charge_time++)
        {
            int left_time = times[race] - charge_time;
            int distanceTravelled = left_time * charge_time;
            if (distanceTravelled > distances[race])
            {
                numWays++;
            }
        }
        */

        // Clever way
        result *= numWays(times[race], distances[race]);
    }

    Console.WriteLine(result);
    Console.ReadLine();
}

void P2()
{
    Int64 result = 0;
    /*
    // Quick and dirty to get a solution quickly
    for (Int64 charge_time = 0; charge_time < timeP2; charge_time++)
    {
        Int64 left_time = timeP2 - charge_time;
        Int64 distanceTravelled = left_time * charge_time;
        if (distanceTravelled > distanceP2)
        {
            result++;
        }
    }
    */

    // Clever way
    result = numWays(timeP2, distanceP2);

    Console.WriteLine(result);
    Console.ReadLine();
}

P1();
P2();
/*
var p1 = Stopwatch.StartNew();
for (Int64 i = 0; i < 10_000_000; i++)
{
    P1();
}
p1.Stop();
var p2 = Stopwatch.StartNew();
for (Int64 i = 0; i < 10_000_000; i++)
{
    P2();
}
p2.Stop();
Console.WriteLine((p1.ElapsedMilliseconds * 1_000_000) / (double)10_000_000);
Console.WriteLine((p2.ElapsedMilliseconds * 1_000_000) / (double)10_000_000);
*/
