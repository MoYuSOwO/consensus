using Godot;
using System;

namespace Consensus.Models;

public partial class TickManager : Node
{
	public static TickManager? Instance { get; private set; }

	[Export] public float TickInterval = 0.1f;

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
		Instance = this;

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

	public override void _ExitTree()
	{
		if (Instance == this) Instance = null;
	}
}
