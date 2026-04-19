using Consensus.Models;
using Consensus.Models.Commands;
using Consensus.Nodes.UI;
using Consensus.Utils;
using Godot;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Consensus.Nodes;

public partial class NetworkManager : Node
{
    private readonly Dictionary<string, Robot> registry = [];
    public ImmutableDictionary<string, Robot> Registry => registry.ToImmutableDictionary();
    public ImmutableArray<string> RobotIds => [.. registry.Keys];
    public ImmutableArray<Robot> Robots => [.. registry.Values];

    public LevelUI UI => GetParent().GetNode<LevelUI>("LevelUI");
    
    private readonly List<Command> inFlightPackets = [];

    public bool IsFlightEmpty => inFlightPackets.Count == 0;

    public override void _Ready()
    {
    }

    public void Init(TickManager ticker)
    {
        ticker.TickUpdate += OnTickUpdate;
    }

    public void RegisterRobot(Robot robot)
    {
        registry[robot.RobotId] = robot;
    }

    public void Send(Command command)
    {
        if (!registry.TryGetValue(command.FromRobotId, out Robot? from))
        {
            GD.PrintErr($"[NetworkManager] Robot - {command.FromRobotId} is not exists.");
            return;
        }
        if (!registry.TryGetValue(command.ToRobotId, out Robot? to))
        {
            GD.PrintErr($"[NetworkManager] Robot - {command.ToRobotId} is not exists.");
            return;
        }

        var v = AlgorithmUtil.CalculateInf(to, from, command);

        command.CalculatedArrivalTick = command.SendTick + v.DelayTicks.Value;
        command.CalculatedArrivalStrength = v.Strength.Value;
        command.CalculatedArrivalLossRatio = v.LossProb.Value;
        
        inFlightPackets.Add(command);

        GD.Print($"[NetworkManager] {command.FromRobotId} -> {command.ToRobotId}. distance: {v.Dist}, delay: {v.DelayTicks.Value} Ticks, strength: {command.SendStrength} -> {command.CalculatedArrivalStrength}");
    }

    public void SendAll(IEnumerable<Command> commands)
    {
        foreach (var command in commands)
        {
            Send(command);
        }
    }

    public void SendAll(IList<Command> commands)
    {
        foreach (var command in commands)
        {
            Send(command);
        }
    }

    public void SendAll(params Command[] commands)
    {
        foreach (var command in commands)
        {
            Send(command);
        }
    }

    private void OnTickUpdate(int currentTick)
    {
        var arrived = inFlightPackets.Where(p => p.CalculatedArrivalTick <= currentTick).ToList();
        var sent = inFlightPackets.Where(p => p.SendTick == currentTick).ToList();

        foreach (var command in sent)
        {
            if (registry.TryGetValue(command.FromRobotId, out Robot? value))
            {
                value.GreenParticle.Emitting = true;
            }
            UI.PrintLog($"[T+{TickManager.TickToSecond(currentTick)}] command sent: {command}", "#cccccc");
        }

        foreach (var command in arrived)
        {
            inFlightPackets.Remove(command);
            if (registry.TryGetValue(command.ToRobotId, out Robot? value))
            {
                if (GD.Randf() > command.CalculatedArrivalLossRatio)
                {
                    value.ReceiveCommand(command, UI);
                }
                else
                {
                    value.RedParticle.Emitting = true;
                    UI.PrintLog($"[T+{TickManager.TickToSecond(currentTick)}] command package lost due to connection: {command}", "#ff7b72");
                }
            }
        }
    }

    

}