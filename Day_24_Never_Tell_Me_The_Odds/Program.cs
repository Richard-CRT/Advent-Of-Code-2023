using AdventOfCodeUtilities;
using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();
List<Hailstone> hailstones = inputList.Select(l => new Hailstone(l)).ToList();
List<Line2D> lines2d = hailstones.Select(hs => hs.toLine2D()).ToList();

#if false



 _____            _                                      _      
|  __ \          | |                                    | |     
| |  \/ __ _ _ __| |__   __ _  __ _  ___    ___ ___   __| | ___ 
| | __ / _` | '__| '_ \ / _` |/ _` |/ _ \  / __/ _ \ / _` |/ _ \
| |_\ \ (_| | |  | |_) | (_| | (_| |  __/ | (_| (_) | (_| |  __/
 \____/\__,_|_|  |_.__/ \__,_|\__, |\___|  \___\___/ \__,_|\___|
                               __/ |                            
                              |___/                             


#endif

void P1()
{
    int intersections = 0;
    for (int i = 0; i < hailstones.Count; i++)
    {
        Hailstone hsA = hailstones[i];
        Line2D hsALine2D = hsA.toLine2D();
        for (int j = i + 1; j < hailstones.Count; j++)
        {
            Hailstone hsB = hailstones[j];
            Line2D hsBLine2D = hsB.toLine2D();

            const Int64 winMin = 200000000000000;
            const Int64 winMax = 400000000000000;

            if (hsALine2D.IntersectWithInFuture(hsBLine2D, out decimal hsALambda, out decimal hsBLambda, out (decimal,decimal)? intersectionLocation) && intersectionLocation!.Value.Item1 >= winMin && intersectionLocation!.Value.Item1 <= winMax && intersectionLocation!.Value.Item2 >= winMin && intersectionLocation!.Value.Item2 <= winMax)
                intersections++;
        }
    }

    Console.WriteLine(intersections);
    Console.ReadLine();
}

void P2()
{
    bool intersect = (new Line2D(new(12, 31), new(-1 - -3, -2 - 1))).IntersectWithInFuture(new Line2D(new(20, 19), new(1 - -3, -5 - 1)), out _, out _, out _);

    Int64 N = 0;
    while (true)
    {
        for (Int64 magX = 1; magX < N + 1; magX++)
        {
            Int64 magY = N - magX;

            foreach ((int signX, int signY) in new List<(int, int)>() { (-1, -1), (-1, 1), (1, -1), (1, 1) })
            {
                if (signX != -1 || magX != 0)
                {
                    if (signY != -1 || magY != 0)
                    {
                        Int64 trialX = magX * signX;
                        Int64 trialY = magY * signY;
                        //Console.WriteLine($"{trialX} {trialY}");

                        bool trialSuccess = true;
                        (decimal,decimal)? commonIntersectionLocation = null;
                        for (int i = 0; i < hailstones.Count; i++)
                        {
                            Hailstone hsA = hailstones[i];
                            Line2D hsALine2D = hsA.toLine2D();
                            Line2D adjustedHsALine2D = new(hsALine2D.Position, hsALine2D.Velocity.Adjust(trialX, trialY));

                            for (int j = i + 1; j < hailstones.Count; j++)
                            {
                                Hailstone hsB = hailstones[j];
                                Line2D hsBLine2D = hsB.toLine2D();

                                Line2D adjustedHsBLine2D = new(hsBLine2D.Position, hsBLine2D.Velocity.Adjust(trialX, trialY));

                                if (adjustedHsALine2D.Velocity.X != 0 || adjustedHsALine2D.Velocity.Y != 0)
                                {
                                    if (adjustedHsBLine2D.Velocity.X != 0 || adjustedHsBLine2D.Velocity.Y != 0)
                                    {
                                        if (!adjustedHsALine2D.IntersectWithInFuture(adjustedHsBLine2D, out decimal hsALambda, out decimal hsBLambda, out (decimal,decimal)? intersectionLocation))
                                        {
                                            trialSuccess = false;
                                            break;
                                        }
                                        else
                                        {
                                            if (intersectionLocation is not null)
                                            {
                                                commonIntersectionLocation = intersectionLocation;
                                            }
                                        }
                                    }
                                }

                            }
                            if (!trialSuccess)
                                break;
                        }

                        if (trialSuccess)
                        {
                            Debug.Assert(commonIntersectionLocation is not null);
                            // All intersect X,Y for given rock velocity -trial
                            Int64 rockVelocityX = -trialX;
                            Int64 rockVelocityY = -trialY;

                            decimal? rockVelocityZ = null;
                            trialSuccess = true;
                            Hailstone hs1 = hailstones[0];
                            Hailstone adjustedHs1 = new(hs1.Position, hs1.Velocity.Adjust(trialX, trialY, 0));
                            for (int i = 1; i < hailstones.Count; i++)
                            {
                                Hailstone hs = hailstones[i];
                                Hailstone adjustedHs = new(hs.Position, hs.Velocity.Adjust(trialX, trialY, 0));
                                decimal testRockVelocityZ = adjustedHs1.DeduceZFromIntersectPoint(commonIntersectionLocation.Value, adjustedHs);
                                if (rockVelocityZ is null)
                                    rockVelocityZ = testRockVelocityZ;
                                else if (rockVelocityZ != testRockVelocityZ)
                                {
                                    trialSuccess = false;
                                    break;
                                }
                            }

                            if (trialSuccess)
                            {
                                Debug.Assert(rockVelocityZ is not null);
                                adjustedHs1 = new(hs1.Position, hs1.Velocity.Adjust(trialX, trialY, (Int64)Math.Round(-rockVelocityZ.Value)));
                                decimal k = adjustedHs1.GetK(commonIntersectionLocation.Value);
                                decimal commonIntersectionLocationZ = adjustedHs1.Position.Z + k * adjustedHs1.Velocity.Z;
                                //Console.WriteLine($"{commonIntersectionLocation.Value.Item1} {commonIntersectionLocation.Value.Item2} {commonIntersectionLocationZ}");
                                Console.WriteLine($"{commonIntersectionLocation.Value.Item1 + commonIntersectionLocation.Value.Item2 + commonIntersectionLocationZ}");
                                //Console.WriteLine($"{rockVelocityX} {rockVelocityY} {rockVelocityZ}");
                                return;
                            }
                        }
                    }
                }
            }
        }
        N++;
    }

    int result = 0;
    Console.WriteLine(result);
    Console.ReadLine();
}

P1();
P2();

public struct Vector2D
{
    public Int64 X;
    public Int64 Y;

    public Vector2D(Int64 x, Int64 y)
    {
        X = x;
        Y = y;
    }

    public Vector2D Adjust(Int64 _x, Int64 _y)
    {
        return new Vector2D(X + _x, Y + _y);
    }

    public override string ToString()
    {
        return $"{X}, {Y}";
    }
}

public struct Vector3D
{
    public Int64 X;
    public Int64 Y;
    public Int64 Z;

    public Vector3D(Int64 x, Int64 y, Int64 z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    public Vector3D(string s)
    {
        var split = s.Split(',', StringSplitOptions.RemoveEmptyEntries);
        X = Int64.Parse(split[0]);
        Y = Int64.Parse(split[1]);
        Z = Int64.Parse(split[2]);
    }

    public Vector2D to2D()
    {
        return new Vector2D(X, Y);
    }

    public Vector3D Adjust(Int64 _x, Int64 _y, Int64 _z)
    {
        return new Vector3D(X + _x, Y + _y, Z + _z);
    }

    public override string ToString()
    {
        return $"{X}, {Y}, {Z}";
    }
}

public struct Line2D
{
    public Vector2D Position;
    public Vector2D Velocity;

    public Line2D(Vector2D position, Vector2D velocity)
    {
        Position = position;
        Velocity = velocity;
    }

    public override string ToString()
    {
        return $"{Position} @ {Velocity}";
    }

    public bool IntersectWithInFuture(Line2D otherLine2D, out decimal k, out decimal h, out (decimal,decimal)? intersectionLocation)
    {
        // Position + kVelocity = otherLine2D.Position + hVelocity
        // Position.X + k*Velocity.X = otherLine2D.Position.X + h*otherLine2D.Velocity.X
        // Position.Y + k*Velocity.Y = otherLine2D.Position.Y + h*otherLine2D.Velocity.Y
        // select h == 1

        // Position.X + k*Velocity.X = otherLine2D.Position.X + h*otherLine2D.Velocity.X
        // Position.Y + k*Velocity.Y = otherLine2D.Position.Y + h*otherLine2D.Velocity.Y

        // k*Velocity.X = otherLine2D.Position.X + h*otherLine2D.Velocity.X - Position.X
        // k*Velocity.Y = otherLine2D.Position.Y + h*otherLine2D.Velocity.Y - Position.Y

        // k = (otherLine2D.Position.X + h*otherLine2D.Velocity.X - Position.X) / Velocity.X

        // ((otherLine2D.Position.X + h*otherLine2D.Velocity.X - Position.X) / Velocity.X) * Velocity.Y = otherLine2D.Position.Y + h*otherLine2D.Velocity.Y - Position.Y
        // (Velocity.Y * (otherLine2D.Position.X + h*otherLine2D.Velocity.X - Position.X)) / Velocity.X = otherLine2D.Position.Y + h*otherLine2D.Velocity.Y - Position.Y
        // Velocity.Y * (otherLine2D.Position.X + h*otherLine2D.Velocity.X - Position.X) = Velocity.X * (otherLine2D.Position.Y + h*otherLine2D.Velocity.Y - Position.Y)
        // (Velocity.Y * otherLine2D.Position.X) + (Velocity.Y * h*otherLine2D.Velocity.X) - (Velocity.Y * Position.X) = (Velocity.X * otherLine2D.Position.Y) + (Velocity.X * h*otherLine2D.Velocity.Y) - (Velocity.X * Position.Y)
        // (Velocity.Y * h*otherLine2D.Velocity.X) = (Velocity.X * otherLine2D.Position.Y) + (Velocity.X * h*otherLine2D.Velocity.Y) - (Velocity.X * Position.Y) - (Velocity.Y * otherLine2D.Position.X) + (Velocity.Y * Position.X)
        // (Velocity.Y * h*otherLine2D.Velocity.X) - (Velocity.X * h*otherLine2D.Velocity.Y) = (Velocity.X * otherLine2D.Position.Y) - (Velocity.X * Position.Y) - (Velocity.Y * otherLine2D.Position.X) + (Velocity.Y * Position.X)
        // h * ((Velocity.Y * otherLine2D.Velocity.X) - (Velocity.X * otherLine2D.Velocity.Y)) = (Velocity.X * otherLine2D.Position.Y) - (Velocity.X * Position.Y) - (Velocity.Y * otherLine2D.Position.X) + (Velocity.Y * Position.X)
        // h = ((Velocity.X * otherLine2D.Position.Y) - (Velocity.X * Position.Y) - (Velocity.Y * otherLine2D.Position.X) + (Velocity.Y * Position.X)) / ((Velocity.Y * otherLine2D.Velocity.X) - (Velocity.X * otherLine2D.Velocity.Y))

        // h*otherLine2D.Velocity.X = Position.X + k*Velocity.X - otherLine2D.Position.X
        // h*otherLine2D.Velocity.Y = Position.Y + k*Velocity.Y - otherLine2D.Position.Y

        // h = (k*Velocity.X - otherLine2D.Position.X + Position.X) / otherLine2D.Velocity.X

        // ((k*Velocity.X - otherLine2D.Position.X + Position.X) / otherLine2D.Velocity.X) * otherLine2D.Velocity.Y = Position.Y + k*Velocity.Y - otherLine2D.Position.Y
        // (otherLine2D.Velocity.Y * (k*Velocity.X - otherLine2D.Position.X + Position.X)) / otherLine2D.Velocity.X = Position.Y + k*Velocity.Y - otherLine2D.Position.Y
        // otherLine2D.Velocity.Y * (k*Velocity.X - otherLine2D.Position.X + Position.X) = otherLine2D.Velocity.X * (Position.Y + k*Velocity.Y - otherLine2D.Position.Y)
        // (otherLine2D.Velocity.Y * k*Velocity.X) - (otherLine2D.Velocity.Y * otherLine2D.Position.X) + (otherLine2D.Velocity.Y * Position.X) = (otherLine2D.Velocity.X * Position.Y) + (otherLine2D.Velocity.X * k*Velocity.Y) - (otherLine2D.Velocity.X * otherLine2D.Position.Y)
        // (otherLine2D.Velocity.Y * k*Velocity.X) = (otherLine2D.Velocity.X * Position.Y) + (otherLine2D.Velocity.X * k*Velocity.Y) - (otherLine2D.Velocity.X * otherLine2D.Position.Y) + (otherLine2D.Velocity.Y * otherLine2D.Position.X) - (otherLine2D.Velocity.Y * Position.X)
        // (otherLine2D.Velocity.Y * k*Velocity.X) - (otherLine2D.Velocity.X * k*Velocity.Y) = (otherLine2D.Velocity.X * Position.Y) - (otherLine2D.Velocity.X * otherLine2D.Position.Y) + (otherLine2D.Velocity.Y * otherLine2D.Position.X) - (otherLine2D.Velocity.Y * Position.X)
        // k * ((otherLine2D.Velocity.Y * Velocity.X) - (otherLine2D.Velocity.X * Velocity.Y)) = (otherLine2D.Velocity.X * Position.Y) - (otherLine2D.Velocity.X * otherLine2D.Position.Y) + (otherLine2D.Velocity.Y * otherLine2D.Position.X) - (otherLine2D.Velocity.Y * Position.X)
        // k = ((otherLine2D.Velocity.X * Position.Y) - (otherLine2D.Velocity.X * otherLine2D.Position.Y) + (otherLine2D.Velocity.Y * otherLine2D.Position.X) - (otherLine2D.Velocity.Y * Position.X)) / ((otherLine2D.Velocity.Y * Velocity.X) - (otherLine2D.Velocity.X * Velocity.Y))

        Int64 h_denominator = (Velocity.Y * otherLine2D.Velocity.X) - (Velocity.X * otherLine2D.Velocity.Y);
        Int64 h_numerator = (Velocity.X * otherLine2D.Position.Y) - (Velocity.X * Position.Y) - (Velocity.Y * otherLine2D.Position.X) + (Velocity.Y * Position.X);
        Int64 k_denominator = (otherLine2D.Velocity.Y * Velocity.X) - (otherLine2D.Velocity.X * Velocity.Y);
        Int64 k_numerator = (otherLine2D.Velocity.X * Position.Y) - (otherLine2D.Velocity.X * otherLine2D.Position.Y) + (otherLine2D.Velocity.Y * otherLine2D.Position.X) - (otherLine2D.Velocity.Y * Position.X);
        if (h_denominator != 0)
        {
            h = (decimal)h_numerator / h_denominator;
            //k = (otherLine2D.Position.X + h * otherLine2D.Velocity.X - Position.X) / Velocity.X;
            k = (decimal)k_numerator / k_denominator;
            intersectionLocation = ((otherLine2D.Position.X + h * otherLine2D.Velocity.X), (otherLine2D.Position.Y + h * otherLine2D.Velocity.Y));
            if (h >= 0 && k >= 0)
                return true;
            else
                return false;
        }
        else
        {
            // lines are parallel
            // check if lines are same

            // Position = otherLine2D.Position + j*otherLine2D.Velocity
            // Position.X = otherLine2D.Position.X + j*otherLine2D.Velocity.X
            // Position.Y = otherLine2D.Position.Y + j*otherLine2D.Velocity.Y

            // j = (Position.X - otherLine2D.Position.X) / otherLine2D.Velocity.X


            // otherLine2D.Position = Position + g*Velocity
            // otherLine2D.Position.X = Position.X + g*Velocity.X
            // otherLine2D.Position.Y = Position.Y + g*Velocity.Y

            // g = (otherLine2D.Position.X - Position.X) / Velocity.X

            if (otherLine2D.Velocity.X == 0)
            {
                if (Position.X == otherLine2D.Position.X)
                {
                    // lines are the same
                    // Doesn't test if intersection is in the past, but oh well
                    h = 0;
                    k = 0;
                    intersectionLocation = null;
                    return true;
                }
                else
                {
                    h = 0;
                    k = 0;
                    intersectionLocation = null;
                    return false;
                }
            }
            else
            {
                decimal j = (Position.X - otherLine2D.Position.X) / (decimal)otherLine2D.Velocity.X;

                //decimal g = (otherLine2D.Position.X - Position.X) / (decimal)Velocity.X;

                if (Position.Y == otherLine2D.Position.Y + j * otherLine2D.Velocity.Y)
                {
                    // lines are the same
                    // Doesn't test if intersection is in the past, but oh well
                    h = 0;
                    k = 0;
                    intersectionLocation = null;
                    return true;
                }
                else
                {
                    h = 0;
                    k = 0;
                    intersectionLocation = null;
                    return false;
                }
            }
        }
    }
}

public struct Hailstone
{
    public Vector3D Position;
    public Vector3D Velocity;

    public Hailstone(string s)
    {
        var split = s.Split('@', StringSplitOptions.RemoveEmptyEntries);
        Position = new(split[0]);
        Velocity = new(split[1]);
    }
    public Hailstone(Vector3D position, Vector3D velocity)
    {
        Position = position;
        Velocity = velocity;
    }

    public Line2D toLine2D()
    {
        return new(Position.to2D(), Velocity.to2D());
    }

    public decimal GetK((decimal, decimal) intersectPoint)
    {
        // intersectPoint.X = Position.X + k * Velocity.X
        // k = (intersectPoint.X - Position.X) / Velocity.X
        decimal k = ((decimal)intersectPoint.Item1 - Position.X) / Velocity.X;
        // Assumes velocity.X is not 0, but didn't come up as error for my input. If this errors, fall back to using Y instead
        return k;
    }

    public decimal DeduceZFromIntersectPoint((decimal, decimal) intersectPoint, Hailstone otherHailstone)
    {
        decimal k = GetK(intersectPoint);
        decimal otherK = otherHailstone.GetK(intersectPoint);
        decimal denominator = k - otherK;
        decimal numerator = Position.Z - otherHailstone.Position.Z + k * Velocity.Z - otherK * otherHailstone.Velocity.Z;
        return numerator / denominator;
    }

    public override string ToString()
    {
        return $"{Position} @ {Velocity}";
    }
}