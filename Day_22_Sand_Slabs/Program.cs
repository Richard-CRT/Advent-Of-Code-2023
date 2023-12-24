using AdventOfCodeUtilities;
using System.Diagnostics;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();

List<Brick> bricks = new();
Dictionary<(int, int, int), Cube> map = new();

foreach (string s in inputList)
{
    var split = s.Split('~', StringSplitOptions.RemoveEmptyEntries);
    Cube cube1 = new(split[0]);
    Cube cube2 = new(split[1]);
    Brick brick = new(bricks.Count);

    int lowerX = Math.Min(cube1.X, cube2.X);
    int higherX = Math.Max(cube1.X, cube2.X);
    int lowerY = Math.Min(cube1.Y, cube2.Y);
    int higherY = Math.Max(cube1.Y, cube2.Y);
    int lowerZ = Math.Min(cube1.Z, cube2.Z);
    int higherZ = Math.Max(cube1.Z, cube2.Z);

    brick.CubeTL = new(lowerX, lowerY, higherZ);
    brick.CubeBR = new(higherX, higherY, lowerZ);

    /*
    if (cube1.X == cube2.X && cube1.Y == cube2.Y && cube1.Z == cube2.Z)
    {
        map[(cube1.X, cube1.Y, cube1.X)] = cube1;
        brick.Cubes.Add(cube1);
    }
    else
    {

        for (int z = lowerZ; z <= higherZ; z++)
        {
            for (int y = lowerY; y <= higherY; y++)
            {
                for (int x = lowerX; x <= higherX; x++)
                {
                    Cube cube = new(x, y, z);
                    map[(x, y, z)] = cube;
                    brick.Cubes.Add(cube);
                }
            }
        }
    }
    */

    bricks.Add(brick);
}


void P1()
{
    int result = 0;

    bool anyMoved = true;
    while (anyMoved)
    {
        anyMoved = false;
        foreach (Brick brick in bricks)
        {
            if (brick.CanMoveDown(bricks))
            {
                anyMoved = true;
                brick.MoveDown();
            }
        }
    }

    int totalNumberThatCanBeDisintegrated = 0;
    for (int i = 0; i < bricks.Count; i++)
    {
        Brick trialBrick = bricks[i];
        List<Brick> otherBricksSupported = trialBrick.SupportsAnyOtherBricks(bricks);
        if (otherBricksSupported.Count == 0)
        {
            totalNumberThatCanBeDisintegrated++;
        }
        else
        {
            bool canBeDisintegrated = true;
            foreach (Brick brickSupportedByTrialBrick in otherBricksSupported)
            {
                List<Brick> bricksThatSupportTheBrickAboveTrialBrock = brickSupportedByTrialBrick.SupportedBy(bricks);
                if (bricksThatSupportTheBrickAboveTrialBrock.Count <= 1)
                {
                    canBeDisintegrated = false;
                    break;
                }
            }
            if (canBeDisintegrated)
                totalNumberThatCanBeDisintegrated++;
        }
    }

    Console.WriteLine(totalNumberThatCanBeDisintegrated);
    Console.ReadLine();
}

void P2()
{
    /*
    int totalNumberDisintegrated = 0;
    int numberDisintegrated = 1;
    while (numberDisintegrated > 0)
    {
        numberDisintegrated = 0;
        for (int i = 0; i < bricks.Count;)
        {
            Brick trialBrick = bricks[i];
            int numberOfOtherBricksSupported = trialBrick.SupportsAnyOtherBricks(bricks);
            if (numberOfOtherBricksSupported == 0)
            {
                numberDisintegrated++;
                bricks.RemoveAt(i);
            }
            else
                i++;
        }
        totalNumberDisintegrated++;
    }
    */

    int result = 0;
    int lastPerc = -1;
    for (int i = 0; i < bricks.Count; i++)
    {
        int perc = (100 * (i)) / (bricks.Count);
        if (perc != lastPerc)
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, currentLineCursor - 1);
            Console.WriteLine($"[{"".PadLeft(perc, '=').PadRight(100, ' ')}] {perc.ToString().PadLeft(3)}%");
            lastPerc = perc;
        }
        Brick trialBrick = bricks[i];
        List<Brick> copyAllBricks = new(bricks);
        copyAllBricks.Remove(trialBrick);
        bool anyChange = true;
        while (anyChange)
        {
            anyChange = false;

            for (int j = 0; j < copyAllBricks.Count; j++)
            {
                if (copyAllBricks[j].CanMoveDown(copyAllBricks))
                {
                    anyChange = true;
                    copyAllBricks.RemoveAt(j);
                    break;
                }
            }
        }
        int howManyWillFall = (bricks.Count - copyAllBricks.Count) - 1;
        result += howManyWillFall;
    }

    Console.WriteLine(result);
    Console.ReadLine();
}

P1();
P2();

public class Brick
{
    public Cube? CubeTL = null;
    public Cube? CubeBR = null;
    public int Index;
    //public List<Cube> Cubes = new();

    public Brick(int index)
    {
        Index = index;
    }

    public void MoveDown()
    {
        CubeTL!.Z--;
        CubeBR!.Z--;
    }

    public List<Brick> SupportedBy(List<Brick> allBricks)
    {
        List<Brick> otherBricksThatSupportThis = new();
        foreach (Brick otherBrick in allBricks)
        {
            if (otherBrick != this)
            {
                if (this.SupportedBy(otherBrick))
                {
                    otherBricksThatSupportThis.Add(otherBrick);
                }
            }
        }
        return otherBricksThatSupportThis;
    }

    public bool SupportedBy(Brick otherBrick)
    {
        int otherBrickTopPlaneZ = otherBrick.CubeTL!.Z;
        int thisBrickBottomPlaneZ = this.CubeBR!.Z;
        if (otherBrickTopPlaneZ == thisBrickBottomPlaneZ - 1)
        {
            // check if square cross section overlaps
            if ((this.CubeTL!.X <= otherBrick.CubeTL!.X && this.CubeBR.X >= otherBrick.CubeTL!.X) ||
                (this.CubeTL!.X <= otherBrick.CubeBR!.X && this.CubeBR.X >= otherBrick.CubeBR!.X) ||
                (this.CubeTL!.X >= otherBrick.CubeTL!.X && this.CubeBR.X <= otherBrick.CubeBR!.X))
            {
                if ((this.CubeTL!.Y <= otherBrick.CubeTL!.Y && this.CubeBR.Y >= otherBrick.CubeTL!.Y) ||
                    (this.CubeTL!.Y <= otherBrick.CubeBR!.Y && this.CubeBR.Y >= otherBrick.CubeBR!.Y) ||
                    (this.CubeTL!.Y >= otherBrick.CubeTL!.Y && this.CubeBR.Y <= otherBrick.CubeBR!.Y))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public List<Brick> SupportsAnyOtherBricks(List<Brick> allBricks)
    {
        List<Brick> otherBricksSupported = new();
        foreach (Brick otherBrick in allBricks)
        {
            if (otherBrick != this)
            {
                if (otherBrick.SupportedBy(this))
                {
                    otherBricksSupported.Add(otherBrick);
                }
            }
        }
        return otherBricksSupported;
    }

    public bool CanMoveDown(List<Brick> allBricks)
    {
        if (CubeBR!.Z > 1)
        {
            foreach (Brick otherBrick in allBricks)
            {
                if (this != otherBrick)
                {
                    if (this.SupportedBy(otherBrick))
                        return false;
                }
            }
            return true;
        }
        return false;
    }

    public override string ToString()
    {
        return $"({CubeTL},{CubeBR})";
    }
}

public class Cube
{
    public int X;
    public int Y;
    public int Z;

    public Cube(string s)
    {
        var coordSplit = s.Split(',', StringSplitOptions.RemoveEmptyEntries);
        X = int.Parse(coordSplit[0]);
        Y = int.Parse(coordSplit[1]);
        Z = int.Parse(coordSplit[2]);
    }
    public Cube(int x, int y, int z)
    {
        X = x; Y = y; Z = z;
    }

    public override string ToString()
    {
        return $"({X},{Y},{Z})";
    }
}