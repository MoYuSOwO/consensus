using Godot;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Consensus.Models;

public partial class NetworkManager : Node
{
    public static NetworkManager? Instance { get; private set; }
    
    private readonly Dictionary<string, Robot> registry = [];
    public ReadOnlyDictionary<string, Robot> Registry => new(registry);
    
    private readonly List<Command> inFlightPackets = [];

    public override void _Ready()
    {
        Instance = this;
    }

    public void RegisterRobot(Robot robot)
    {
        registry[robot.RobotId] = robot;
    }

    public void SendPacket(Command packet)
    {
        if (!registry.TryGetValue(packet.FromRobotId, out Robot? fromNode) || !registry.TryGetValue(packet.ToRobotId, out Robot? toNode)) return;

        // distance
        float dist = fromNode.GlobalPosition.DistanceTo(toNode.GlobalPosition);
        
        // delay
        int delayTicks = Mathf.CeilToInt(dist / 100.0f);
        if (delayTicks < 1) delayTicks = 1; 

        packet.CalculatedArrivalTick = packet.SendTick + delayTicks;
        inFlightPackets.Add(packet);

        GD.Print($"[NetworkManager] {packet.FromRobotId} -> {packet.ToRobotId}. distance: {dist}, delay: {delayTicks} Ticks.");
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

    public override void _ExitTree()
	{
		if (Instance == this) Instance = null;
	}
}