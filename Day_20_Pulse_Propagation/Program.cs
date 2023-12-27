using AdventOfCodeUtilities;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

List<string> inputList = AoC.GetInputLines();
List<Module> modules = inputList.Select(s => Module.Create(s)).ToList();
modules.ForEach(module => module.OutputModulesNames.ForEach(outputModuleName => { if (!Module.ModulesByName.ContainsKey(outputModuleName)) Module.ModulesByName[outputModuleName] = new UntypedModule(outputModuleName); }));
modules.ForEach(module => module.AssociateModulesByName());

void P1()
{
    Queue<Pulse> pulses = new();

    Int64 lowPulseCount = 0;
    Int64 highPulseCount = 0;
    for (int i = 0; i < 1000; i++)
    {
        pulses.Enqueue(new Pulse(null, Module.ModulesByName["broadcaster"], false));
        while (pulses.Any())
        {
            Pulse nextPulse = pulses.Dequeue();
            if (nextPulse.High)
                highPulseCount++;
            else
                lowPulseCount++;
            //Console.WriteLine(nextPulse.ToString());
            if (nextPulse.ReceivingModule is not null)
                nextPulse.ReceivingModule.Pulse(nextPulse.SendingModule, nextPulse.High).ForEach(pulse => pulses.Enqueue(pulse)); ;
        }
        //Console.WriteLine();
    }

    Console.WriteLine(highPulseCount * lowPulseCount);
    Console.ReadLine();
}

void P2()
{
    modules.ForEach(module => module.Reset());

    Queue<Pulse> pulses = new();
    Module endMachine = Module.ModulesByName["rx"]!;

    // Need to figure out the first stage where the output stops being one and becomes a conjunction of many
    Module currentModule = endMachine;
    while (currentModule.InputModules.Count == 1)
        currentModule = currentModule.InputModules[0];
    var lastModuleWithMultipleInputs = currentModule;

    // For my example, currentModule is now &bq, as its inputs are multiple conjunction modules
    // We assume these inputs are the ones we need to determine cycle lengths for
    HashSet<Module> modulesWeNeedToDetermineCycleLengthFor = lastModuleWithMultipleInputs.InputModules.ToHashSet();
    Dictionary<Module, Int64> cycleLength = new();

    int buttonPresses;
    bool rxLowPulseFound = false;
    Int64 result;
    for (buttonPresses = 0; true; buttonPresses++)
    {
        pulses.Enqueue(new Pulse(null, Module.ModulesByName["broadcaster"], false));
        while (pulses.Any())
        {
            Pulse nextPulse = pulses.Dequeue();
            var sM = nextPulse.SendingModule;
            if (sM is not null && modulesWeNeedToDetermineCycleLengthFor.Contains(sM))
            {
                if (!cycleLength.ContainsKey(sM) && nextPulse.High)
                {
                    cycleLength[sM] = buttonPresses + 1;
                }
            }
            if (nextPulse.ReceivingModule == endMachine && !nextPulse.High)
            {
                // Not going to happen in realistic time
                rxLowPulseFound = true;
                break;
            }
            if (nextPulse.ReceivingModule is not null)
                nextPulse.ReceivingModule.Pulse(nextPulse.SendingModule, nextPulse.High).ForEach(pulse => pulses.Enqueue(pulse)); ;
        }
        
        // For my example
        // &lx, &db, &qz, &sd -> &vg, &kp, &gc, &tx -> &bq -> &rx
        // To send a low to rx, all inputs to bq need to be HIGH
        // For all inputs to bq to be HIGH, &vg, &kp, &gc, &tx need to output HIGH
        // Everything after this is irrelevant for my slightly generic solution in the end, other than the final comment line
        // For &vg, &kp, &gc, &tx to output HIGH, &lx, &db, &qz, &sd need to output LOW
        // For &lx, &db, &qz, &sd to output LOW, all their flip-flop inputs need to be HIGH
        // &lx = {lg,gf,bm,cp,xm,kh,lh,dl,zx,gb -> &lx -> fn,hf,vg,lg}
        // &db = {vv,sp,bh,kr,xz,qf,mq,zs -> &db -> kg,sp,kp,fx,jh,gz}
        // &qz = {ql,vn,gs,xb,gq,fs,vh -> &qz -> nd,sj,sk,gp,gc,vh,zt}
        // &sd = {tr,xp,hl,mh,cv,sv,pz,cn -> &sd -> mh,tx,sh,xf,zn,xs}
        // Printing these flip-flop outputs as a string shows binary progression, which makes sense, let's look for LCM of the higher level modules

        if (cycleLength.Keys.Count == modulesWeNeedToDetermineCycleLengthFor.Count)
        {
            long lcm = AoC.LCM(cycleLength.Values.ToArray());
            result = lcm;
            break;
        }

        // Not going to happen in realistic time
        if (rxLowPulseFound)
        {
            result = buttonPresses + 1;
            break;
        }
    }

    Console.WriteLine(result);
    Console.ReadLine();
}

P1();
P2();

public class Pulse
{
    public Module? SendingModule;
    public Module ReceivingModule;
    public bool High;

    public Pulse(Module? sendingModule, Module receivingModule, bool high)
    {
        SendingModule = sendingModule;
        ReceivingModule = receivingModule;
        High = high;
    }

    public override string ToString()
    {
        return $"{(SendingModule is null ? "button" : SendingModule.Name)} -{(High ? "high" : "low")}-> {(ReceivingModule is null ? "output" : ReceivingModule.Name)}";
    }
}
public class UntypedModule : Module
{
    public override string TypeString { get { return ""; } }

    public UntypedModule(string name) : base(name)
    {

    }

    public override List<Pulse> Pulse(Module? sendingModule, bool high)
    {
        return new();
    }

    public override void Reset()
    {
    }
}

public class BroadcasterModule : Module
{
    public override string TypeString { get { return ""; } }

    public BroadcasterModule(string name) : base(name)
    {

    }

    public override List<Pulse> Pulse(Module? sendingModule, bool high)
    {
        List<Pulse> pulses = OutputModules.Select(outputModule => new Pulse(this, outputModule, high)).ToList();
        return pulses;
    }

    public override void Reset()
    {
    }
}

public class FlipFlopModule : Module
{
    public override string TypeString { get { return "%"; } }

    public bool On;

    public FlipFlopModule(string name) : base(name)
    {
        Reset();
    }

    public override List<Pulse> Pulse(Module? sendingModule, bool high)
    {
        Debug.Assert(sendingModule is not null);
        if (!high)
        {
            On = !On;
            return OutputModules.Select(outputModule => new Pulse(this, outputModule, On)).ToList();
        }
        else
            return new();
    }

    public override void Reset()
    {
        On = false;
    }
}

public class ConjunctionModule : Module
{
    public override string TypeString { get { return "&"; } }
    public Dictionary<Module, bool> Memory = new();

    public ConjunctionModule(string name) : base(name)
    {

    }

    public override void AssociateModulesByName()
    {
        base.AssociateModulesByName();
        Reset();
    }

    public override List<Pulse> Pulse(Module? sendingModule, bool high)
    {
        Debug.Assert(sendingModule is not null);
        Memory[sendingModule] = high;
        bool allHigh = Memory.Values.Aggregate(true, (acc, mem) => mem ? acc : false);
        return OutputModules.Select(outputModule => new Pulse(this, outputModule, !allHigh)).ToList();
    }

    public override void Reset()
    {
        Memory = InputModules.ToDictionary(inputModule => inputModule, inputModule => false);
    }
}

public abstract class Module
{
    public abstract string TypeString { get; }
    public static Dictionary<string, Module> ModulesByName = new();

    public static Module Create(string s)
    {
        //%b -> c
        var split = s.Split(" -> ");

        Module newModule;
        if (split[0] == "broadcaster")
        {
            newModule = new BroadcasterModule("broadcaster");
        }
        else
        {
            if (split[0][0] == '%')
                newModule = new FlipFlopModule(split[0].Substring(1));
            else
                newModule = new ConjunctionModule(split[0].Substring(1));
        }

        var split2 = split[1].Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        newModule.OutputModulesNames = split2.ToList();

        ModulesByName[newModule.Name] = newModule;
        return newModule;
    }

    public string Name;
    public List<Module> InputModules = new();
    public List<string> OutputModulesNames = new();
    public List<Module> OutputModules = new();

    public Module(string name)
    {
        Name = name;
    }

    public virtual void AssociateModulesByName()
    {
        OutputModules = OutputModulesNames.Select(outputModuleName => ModulesByName[outputModuleName]).ToList();
        OutputModules.ForEach(outputModule => { if (outputModule is not null) outputModule.InputModules.Add(this); });
    }

    public abstract List<Pulse> Pulse(Module? sendingModule, bool high);
    public abstract void Reset();

    public override string ToString()
    {
        return $"{string.Join(',', InputModules.Select(inputModule => inputModule.Name))} -> {TypeString}{Name} -> {string.Join(',', OutputModules.Select(outputModule => outputModule.Name))}";
    }
}