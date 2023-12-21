using AdventOfCodeUtilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();
List<Instruction> instructions = new List<Instruction>();
List<Part> parts = new List<Part>();
int i = 0;
while (inputList[i] != "")
{
    instructions.Add(new(inputList[i]));
    i++;
}
i++;
while (i < inputList.Count)
{
    parts.Add(new(inputList[i]));
    i++;
}
Instruction.InstructionsByName = new(instructions.ToDictionary(instruction => instruction.Name, instruction => instruction));
foreach (Instruction inst in instructions)
{
    foreach (var groups in inst.ConditionActionPairsByName)
    {
        if (groups.Item2 == Action.Forward)
        {
            Instruction otherInst = Instruction.InstructionsByName[groups.Item3!];
            inst.ConditionActionPairs.Add((groups.Item1, Action.Forward, otherInst));
            otherInst.ConditionsThatForwardHere.Add(groups.Item1);
        }
        else
            inst.ConditionActionPairs.Add((groups.Item1, groups.Item2, null));
    }
}

List<List<Condition>> recurse(Condition condition)
{
    List<List<Condition>> conditionChains = new();
    if (condition.ParentInstruction.Name == "in")
    {
        List<Condition> chainToGetHere = new();
        int indexOfThisConditionInParent = condition.ParentInstruction.ConditionActionPairs.FindIndex(cAP => cAP.Item1 == condition);
        chainToGetHere.Add(condition);
        for (int i = indexOfThisConditionInParent - 1; i >= 0; i--)
            chainToGetHere.Add(new (condition.ParentInstruction.ConditionActionPairs[i].Item1, true));
        conditionChains.Add(chainToGetHere);
    }
    else
    {
        List<Condition> chainToGetHere = new();
        int indexOfThisConditionInParent = condition.ParentInstruction.ConditionActionPairs.FindIndex(cAP => cAP.Item1 == condition);
        chainToGetHere.Add(condition);
        for (int i = indexOfThisConditionInParent - 1; i >= 0; i--)
            chainToGetHere.Add(new (condition.ParentInstruction.ConditionActionPairs[i].Item1, true));
        foreach (Condition iterCond in condition.ParentInstruction.ConditionsThatForwardHere)
        {
            recurse(iterCond).ForEach(l => conditionChains.Add((new List<Condition>(chainToGetHere)).Concat(l).ToList()));
        }
    }
    return conditionChains;
}

void P1()
{
    int result = parts.Aggregate(0, (acc, part) => Instruction.InstructionsByName["in"].Evaluate(part) == Action.Accept ? acc + part.A + part.M + part.S + part.X : acc);
    Console.WriteLine(result);
    Console.ReadLine();
}

void P2()
{
    List<Condition> acceptConditions = new();
    foreach (Instruction inst in instructions)
    {
        foreach ((Condition cond, Action act, Instruction? forwardInst) in inst.ConditionActionPairs)
        {
            if (act == Action.Accept)
            {
                acceptConditions.Add(cond);
            }
        }
    }

    List<List<Condition>> conditionChains = new();
    foreach (Condition condition in acceptConditions)
    {
        conditionChains.AddRange(recurse(condition));
    }

    //conditionChains.ForEach(cc => cc.Reverse());

    Int64 result = 0;
    foreach (var chain in conditionChains)
    {
        //Console.WriteLine(string.Join(' ', chain));
        int minS = 1;
        int maxS = 4000;
        int minX = 1;
        int maxX = 4000;
        int minA = 1;
        int maxA = 4000;
        int minM = 1;
        int maxM = 4000;
        foreach (Condition cond in chain)
        {
            if (cond.Operator != Operator.True)
            {
                Func<int, int, (int,int)> mathFunc;
                switch (cond.Operator)
                {
                    case Operator.LessThan: mathFunc = (minVal, maxVal) => (minVal, Math.Min(maxVal, cond.Value - 1)); break;
                    case Operator.GreaterThan: mathFunc = (minVal, maxVal) => (Math.Max(minVal, cond.Value + 1), maxVal); break;
                    case Operator.LessThanOrEqualTo: mathFunc = (minVal, maxVal) => (minVal, Math.Min(maxVal, cond.Value)); break;
                    case Operator.GreaterThanOrEqualTo: mathFunc = (minVal, maxVal) => (Math.Max(minVal, cond.Value), maxVal); break;
                    default: throw new Exception();
                }
                switch (cond.Variable)
                {
                    case 'x': (minX, maxX) = mathFunc(minX, maxX); break;
                    case 'a': (minA, maxA) = mathFunc(minA, maxA); break;
                    case 's': (minS, maxS) = mathFunc(minS, maxS); break;
                    case 'm': (minM, maxM) = mathFunc(minM, maxM); break;
                }
            }
        }
        //Console.WriteLine($"{minX} ≤ X ≤ {maxX}");
        //Console.WriteLine($"{minA} ≤ A ≤ {maxA}");
        //Console.WriteLine($"{minS} ≤ S ≤ {maxS}");
        //Console.WriteLine($"{minM} ≤ M ≤ {maxM}");
        Int64 numX = maxX - minX + 1;
        Int64 numA = maxA - minA + 1;
        Int64 numS = maxS - minS + 1;
        Int64 numM = maxM - minM + 1;
        Int64 prod = numX *numA * numS * numM;
        //Console.WriteLine(prod);
        result += prod;
    }

    Console.WriteLine(result);
    Console.ReadLine();
}

P1();
P2();

public enum Action
{
    Accept,
    Reject,
    Forward
}

public enum Operator
{
    True = '*',
    GreaterThan = '>',
    LessThan = '<',
    LessThanOrEqualTo = '≤',
    GreaterThanOrEqualTo = '≥',
}

public class Condition
{
    public Instruction? ParentInstruction;

    public char Variable;
    public int Value;
    public Operator Operator;
    public Func<int, bool>? Function;

    public Condition(Instruction parentInstruction, string s)
    {
        ParentInstruction = parentInstruction;

        Variable = s[0];
        Operator = (Operator)s[1];
        Value = int.Parse(s[2..]);
    }

    public Condition(Condition cond, bool invert)
    {
        this.ParentInstruction = null;
        this.Variable = cond.Variable;
        this.Value = cond.Value;
        if (invert)
        {
            switch (cond.Operator)
            {
                case Operator.LessThan: this.Operator = Operator.GreaterThanOrEqualTo; break;
                case Operator.GreaterThan: this.Operator = Operator.LessThanOrEqualTo; break;
                case Operator.LessThanOrEqualTo: this.Operator = Operator.GreaterThan; break;
                case Operator.GreaterThanOrEqualTo: this.Operator = Operator.LessThan; break;
                default: throw new Exception();
            }
        }
        else
        {
            this.Operator = cond.Operator;
        }
    }

    /*
    public bool Evaluate(Part part)
    {
        switch (Operator)
        {
            case Operator.True: return true;
            case Operator.LessThan: return part[Variable] < Value;
            case Operator.GreaterThan: return part[Variable] > Value;
            default: throw new Exception();
        }
    }
    */
    public bool Evaluate(Part part)
    {
        if (Function is null)
        {
            switch (Operator)
            {
                case Operator.True: Function = (int val) => true; break;
                case Operator.LessThan: Function = (int val) => val < Value; break;
                case Operator.GreaterThan: Function = (int val) => val > Value; break;
                default: throw new Exception();
            }
        }
        return Function(part[Variable]);
    }

    public override string ToString()
    {
        if (Operator == Operator.True)
            return $"*";
        else
            return $"{Variable}{(char)Operator}{Value}";
    }
}

public class Instruction
{
    public static Dictionary<string, Instruction> InstructionsByName = new();

    public string Name;
    public List<(Condition, Action, Instruction?)> ConditionActionPairs = new();
    public List<(Condition, Action, string?)> ConditionActionPairsByName = new();
    public List<Condition> ConditionsThatForwardHere = new();

    public Instruction(string s)
    {
        var split = s.Trim('}').Split('{', StringSplitOptions.RemoveEmptyEntries);
        Name = split[0];
        var split2 = split[1].Split(',', StringSplitOptions.RemoveEmptyEntries);
        Debug.Assert(split2.Length > 0);
        foreach (string scond in split2)
        {
            int indexOfColon = scond.IndexOf(':');
            if (indexOfColon != -1)
            {
                Condition cond = new(this, scond.Substring(0, indexOfColon));
                string actString = scond.Substring(indexOfColon + 1);
                if (actString[0] == 'A')
                {
                    ConditionActionPairsByName.Add((cond, Action.Accept, null));
                }
                else if (actString[0] == 'R')
                {
                    ConditionActionPairsByName.Add((cond, Action.Reject, null));
                }
                else
                {
                    ConditionActionPairsByName.Add((cond, Action.Forward, actString));
                }
            }
            else
            {
                Condition cond = new(this, "x*0"); // always true
                if (scond[0] == 'A')
                    ConditionActionPairsByName.Add((cond, Action.Accept, null));
                else if (scond[0] == 'R')
                    ConditionActionPairsByName.Add((cond, Action.Reject, null));
                else
                    ConditionActionPairsByName.Add((cond, Action.Forward, scond));
            }
        }
    }

    public Action Evaluate(Part part)
    {
        foreach ((Condition cond, Action act, Instruction? inst) in ConditionActionPairs)
        {
            if (cond.Evaluate(part))
            {
                switch (act)
                {
                    case Action.Accept:
                    case Action.Reject:
                        return act;
                    case Action.Forward:
                        return inst!.Evaluate(part);
                    default:
                        throw new Exception();
                }
            }
        }
        throw new Exception();
    }

    public override string ToString()
    {
        return $"{Name}{{{string.Join(',', ConditionActionPairs)}}}";
    }
}

public class Part
{
    public int X;
    public int M;
    public int A;
    public int S;

    public int this[char index]
    {
        get
        {
            switch (index)
            {
                case 'x': return X;
                case 'm': return M;
                case 'a': return A;
                case 's': return S;
                default: throw new Exception();
            }
        }
        set
        {
            switch (index)
            {
                case 'x': X = value; break;
                case 'm': M = value; break;
                case 'a': A = value; break;
                case 's': S = value; break;
                default: throw new Exception();
            }
        }
    }

    public Part(string s)
    {
        var data = s.Trim(new char[] { '{', '}' }).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(split => split.Split('=', StringSplitOptions.RemoveEmptyEntries)).ToArray();
        X = int.Parse(data[0][1]);
        M = int.Parse(data[1][1]);
        A = int.Parse(data[2][1]);
        S = int.Parse(data[3][1]);
    }
}