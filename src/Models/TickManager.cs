using Godot;
using System;

public partial class TickManager : Node
{
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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
