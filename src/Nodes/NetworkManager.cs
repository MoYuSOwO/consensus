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

    public void RegisterRobot(Robot robot)
    {
        registry[robot.RobotId] = robot;
    }

    public void SendPacket(Command packet)
    {
        if (!registry.TryGetValue(packet.FromRobotId, out Robot? from))
        {
            GD.PrintErr($"[NetworkManager] Robot - {packet.FromRobotId} is not exists.");
            return;
        }
        if (!registry.TryGetValue(packet.ToRobotId, out Robot? to))
        {
            GD.PrintErr($"[NetworkManager] Robot - {packet.ToRobotId} is not exists.");
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
            strength = packet.SendStrength;
            lossProb = 0;
        }
        else
        {
            dist = from.GlobalPosition.DistanceTo(to.GlobalPosition);
            delayTicks = AlgorithmUtil.RandomNetworkDelay.Value + AlgorithmUtil.GetRandomRobotDelay(to).Value;
            strength = AlgorithmUtil.GetDecreaseRatio(dist).Value * packet.SendStrength;
            lossProb = AlgorithmUtil.GetLossProb(strength).Value;
        }

        packet.CalculatedArrivalTick = packet.SendTick + delayTicks;
        packet.CalculatedArrivalStrength = strength;
        packet.CalculatedArrivalLossRatio = lossProb;
        
        inFlightPackets.Add(packet);

        GD.Print($"[NetworkManager] {packet.FromRobotId} -> {packet.ToRobotId}. distance: {dist}, delay: {delayTicks} Ticks, strength: {packet.SendStrength} -> {packet.CalculatedArrivalStrength}");
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