using Godot;
using System;

namespace Consensus.Nodes;

public partial class TickManager : Node
{
	public const int TickPerSecond = 10;
	public static float TickInterval => 1.0f / TickPerSecond;

	public int CurrentTick { get; private set; } = 0;
    
    [Signal]
    public delegate void TickUpdateEventHandler(int tick);

    public void OnTimerTimeout() 
    {
        CurrentTick++;
        EmitSignal(SignalName.TickUpdate, CurrentTick);
        GD.Print($"[TickManager] Tick: {CurrentTick}");
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		foreach (var node in GetChildren())
		{
			node.QueueFree();
		}

        Timer timer = new()
        {
            Name = "TickTimer",
			WaitTime = TickInterval,
			OneShot = false,
			Autostart = true
        };
		timer.Timeout += OnTimerTimeout;
        AddChild(timer);

		timer.Start();

		GD.Print($"[TickManager] Timer starts, interval: {TickInterval}");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
}
