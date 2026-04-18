using Godot;
using System;
using System.Collections.Generic;

namespace Consensus.Models;

public partial class Robot : Sprite2D
{
    [Export] public string RobotId { get; set; } = "Robot_A";
    public const int GRID_SIZE = 64;

    private readonly Queue<Command> actionQueue = new();
    public bool IsBusy { get; private set; } = false;

    public override void _Ready()
    {
    }

    public void Init()
    {
        var networkManager = NetworkManager.Instance;
        var tickManager = TickManager.Instance;

        if (networkManager == null) throw new ArgumentNullException("NetworkManager.Instance");
        if (tickManager == null) throw new ArgumentNullException("TickManager.Instance");

        networkManager.RegisterRobot(this);
        tickManager.TickUpdate += OnTickUpdate;
    }

    public void ReceiveCommand(Command cmd)
    {
        if (actionQueue.Count < 3) {
            actionQueue.Enqueue(cmd);
            GD.Print($"[Robot - {RobotId}] Recieved the instrction. Queue length: {actionQueue.Count}");
        } else {
            GD.PrintErr($"[Robot - {RobotId}] queue is full! Instrction is discarded");
        }
    }

    private void OnTickUpdate(int currentTick)
    {
        if (!IsBusy && actionQueue.Count > 0)
        {
            IsBusy = true;
            var cmd = actionQueue.Dequeue();
            cmd.Execute(this);
        }
    }

    public void PerformStep(Vector2I dir)
    {
        Vector2 targetPos = GlobalPosition + new Vector2(dir.X * GRID_SIZE, dir.Y * GRID_SIZE);
        
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "global_position", targetPos, 0.3f);
        
        tween.Finished += () => {
            IsBusy = false;
        };
    }
}