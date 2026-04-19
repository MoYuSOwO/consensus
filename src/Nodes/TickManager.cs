using Consensus.Utils;
using Godot;
using System;

namespace Consensus.Nodes;

public partial class TickManager : Node
{
	public const int TickPerSecond = 10;
	public static float TickInterval => TickToSecond(1);

	public static int SecondToTick(float second)
	{
		return (int)(second * TickPerSecond);
	}

	public static int SecondToTick(double second)
	{
		return (int)(second * TickPerSecond);
	}

	public static float TickToSecond(int tick)
	{
		return (float)tick / (float)TickPerSecond;
	}

	public int CurrentTick { get; private set; } = 0;

	private Timer? _timer;
	private Timer TickTimer => BasicUtil.Must(_timer, "TickManager");
	public bool IsRunning { get; private set; } = false;
    
    [Signal]
    public delegate void TickUpdateEventHandler(int tick);
	[Signal]
	public delegate void TickingStartedEventHandler();
    [Signal]
	public delegate void TickingStoppedEventHandler();

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

		_timer = new Timer
        {
            Name = "TickTimer",
            WaitTime = TickInterval,
            OneShot = false,
            Autostart = false
        };
		_timer.Timeout += OnTimerTimeout;
        AddChild(_timer);

		GD.Print($"[TickManager] Timer is initialized, interval: {TickInterval}, wating to start...");
	}

	public void StartTicking()
    {
        if (IsRunning) return;
        
        IsRunning = true;
        TickTimer.Start();
        EmitSignal(SignalName.TickingStarted);
        GD.Print($"[TickManager] Timer is started, interval: {TickInterval}");
    }

	public void StopTicking()
    {
        if (!IsRunning) return;

        IsRunning = false;
        TickTimer.Stop();
        EmitSignal(SignalName.TickingStopped);
        GD.Print($"[TickManager] Timer is stopped.");
    }

    public void ResetTicks()
    {
        StopTicking();
        CurrentTick = 0;
        GD.Print("[TickManager] Timer and Tick is reseted.");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

}
