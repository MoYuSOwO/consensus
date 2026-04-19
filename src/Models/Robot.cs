using Consensus.Models.Commands;
using Consensus.Models.Exceptions;
using Consensus.Nodes;
using Consensus.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Consensus.Models;

[GlobalClass]
public partial class Robot : Node2D
{
    public string RobotId
    {
        get => Name;
        set => Name = value;
    }
    [Export] public float MinSendDelayTime { get; set; } = 1.0f;
    [Export] public float MaxSendDelayTime { get; set; } = 3.0f;
    
    private string Caller => $"Robot - {RobotId}";

    public Vector2I GridPos { get; protected set; } = new();
    private TileMapLayer? _map;
    public TileMapLayer MapLayer => BasicUtil.Must(_map, Caller);

    private readonly Queue<Command> actionQueue = new();
    public bool IsBusy { get; private set; } = false;
    public bool IsQueueEmpty => actionQueue.Count == 0;

    public Direction FacingDirection { get; private set; } = Direction.Down;

    private NetworkManager? _network;
    public NetworkManager Network => BasicUtil.Must(_network, Caller);
    
    private TickManager? _ticker;
    public TickManager Ticker => BasicUtil.Must(_ticker, Caller);

    public Sprite2D Body => GetNode<Sprite2D>("Body");

    public override void _Ready()
    {
    }

    public void Init(TileMapLayer map, NetworkManager network, TickManager ticker)
    {
        _map = map;
        _network = network;
        _ticker = ticker;

        GridPos = MapLayer.LocalToMap(Position);
        Position = MapLayer.MapToLocal(GridPos);

        Network.RegisterRobot(this);
        Ticker.TickUpdate += OnTickUpdate;
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

    private bool IsCellWall(Vector2I gridPos)
    {
        TileData tileData = MapLayer.GetCellTileData(gridPos);
        return tileData != null && tileData.GetCustomData("is_wall").AsBool();
    }

    public void Step(int length)
    {
        IsBusy = true;

        Vector2I direction = FacingDirection.ToVector2I();
        Vector2I lastValidGrid = GridPos;
        int actualSteps = 0;

        for (int i = 1; i <= length; i++)
        {
            Vector2I nextGrid = GridPos + direction * i;
            if (IsCellWall(nextGrid)) 
            {
                break; 
            }
            lastValidGrid = nextGrid;
            actualSteps = i;
        }

        float basicDuration = TickManager.TickToSecond(StepCommand.TickPerStep);
        float moveDuration = Mathf.Max(actualSteps * basicDuration, basicDuration);

        GridPos = lastValidGrid;
        Vector2 targetPixel = MapLayer.MapToLocal(GridPos);

        var tween = GetTree().CreateTween();
        tween.TweenProperty(this, "position", targetPixel, moveDuration);

        tween.Finished += () => {
            IsBusy = false;
            if (actualSteps < length) GD.Print($"[Robot - {RobotId}] robot is blocked! stop walking...");
        };
    }

    public void RotateTo(Direction dir)
    {
        IsBusy = true;
        FacingDirection = dir;

        int targetFrame = dir.ToFrameId();

        var tween = GetTree().CreateTween();

        float duration = TickManager.TickToSecond(RotateCommand.RotateTick);
        float durationA = duration / 4.0f * 3.0f;
        float durationB = duration - durationA;

        tween.TweenProperty(this, "scale", new Vector2(1.2f, 0.8f), durationA);

        tween.TweenCallback(Callable.From(() => {
            Body.Frame = targetFrame;
        }));

        tween.TweenProperty(this, "scale", Vector2.One, durationB)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);

        tween.Finished += () => {
            IsBusy = false;
        };
    }

    public SceneTreeTimer Wait(float time)
    {
        IsBusy = true;
        var timer = GetTree().CreateTimer(time);
        timer.Timeout += () => {
            IsBusy = false;
        };
        return timer;
    }
}