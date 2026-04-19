using Consensus.Models;
using Consensus.Models.Commands;
using Consensus.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Consensus.Nodes;

public partial class NetworkManager : Node
{
    private readonly Dictionary<string, Robot> registry = [];
    public ReadOnlyDictionary<string, Robot> Registry => new(registry);
    
    private readonly List<Command> inFlightPackets = [];

    public bool IsFlightEmpty => inFlightPackets.Count > 0;

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

        // distance
        float dist;
        
        // delay
        int delayTicks;

        // strength
        float strength;

        // loss
        float lossProb;

        if (to == from)
        {
            dist = 0;
            delayTicks = AlgorithmUtil.RandomNetworkDelay.Value;
            strength = command.SendStrength;
            lossProb = 0;
        }
        else
        {
            dist = from.GlobalPosition.DistanceTo(to.GlobalPosition);
            delayTicks = AlgorithmUtil.RandomNetworkDelay.Value + AlgorithmUtil.GetRandomRobotDelay(to).Value;
            strength = AlgorithmUtil.GetDecreaseRatio(dist).Value * command.SendStrength;
            lossProb = AlgorithmUtil.GetLossProb(strength).Value;
        }

        command.CalculatedArrivalTick = command.SendTick + delayTicks;
        command.CalculatedArrivalStrength = strength;
        command.CalculatedArrivalLossRatio = lossProb;
        
        inFlightPackets.Add(command);

        GD.Print($"[NetworkManager] {command.FromRobotId} -> {command.ToRobotId}. distance: {dist}, delay: {delayTicks} Ticks, strength: {command.SendStrength} -> {command.CalculatedArrivalStrength}");
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

        foreach (var packet in arrived)
        {
            inFlightPackets.Remove(packet);
            if (registry.TryGetValue(packet.ToRobotId, out Robot? value))
            {
                value.ReceiveCommand(packet);
            }
        }
    }

}