using Consensus.Models.Commands;
using Consensus.Models.Exceptions;
using Consensus.Nodes;
using Consensus.Utils;
using Godot;
using System;
using System.Collections.Generic;

namespace Consensus.Models;

public partial class Robot : Sprite2D
{
    [Export] public string RobotId { get; set; } = "Robot_A";
    [Export] public Texture2D? RobotTexture { get; set; }
    [Export] public float MinSendDelayTime { get; set; } = 1.0f;
    [Export] public float MaxSendDelayTime { get; set; } = 3.0f;
    public const int GRID_SIZE = 64;

    public Vector2I GridPos { get; protected set; } = new();
    private TileMapLayer? _map;
    public TileMapLayer MapLayer
    {
        get
        {
            if (_map == null)
            {
                GD.PrintErr($"[Robot - {RobotId}] Robot - MapLayer is not exists.");
                throw new ArgumentNullException("_map");
            }
            return _map;
        }
    }

    private readonly Queue<Command> actionQueue = new();
    public bool IsBusy { get; private set; } = false;
    public bool IsQueueEmpty => actionQueue.Count == 0;

    public Direction FacingDirection { get; private set; } = Direction.Down;

    public NetworkManager Network => GetParent().GetParent().GetParent().GetNode<NetworkManager>("NetworkManager") ?? throw new NotYetInitializationException();
    public TickManager Tick => GetParent().GetParent().GetParent().GetNode<TickManager>("TickManager") ?? throw new NotYetInitializationException();

    public override void _Ready()
    {
        if (RobotTexture != null)
        {
            Texture = RobotTexture;
        }
        Centered = true;
    }

    public void Init(TileMapLayer map, Vector2I startPos)
    {
        _map = map;
        GridPos = startPos;
        Position = _map.MapToLocal(GridPos);

        Network.RegisterRobot(this);
        Tick.TickUpdate += OnTickUpdate;
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

        float moveDuration = Mathf.Max(actualSteps * 0.3f, 0.3f);

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

        float currentDeg = Mathf.RadToDeg(Rotation);
        float targetDeg = dir.ToDegree();

        float diff = targetDeg - currentDeg;
        diff = (float)Math.IEEERemainder(diff, 360);

        float finalTargetDeg = currentDeg + diff;

        var tween = GetTree().CreateTween();
        tween.TweenProperty(this, "rotation_degrees", finalTargetDeg, 0.2f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        tween.Finished += () => {
            RotationDegrees = finalTargetDeg % 360;
            if (RotationDegrees < 0) RotationDegrees += 360;
            
            IsBusy = false;
            GD.Print($"[Robot - {RobotId}] Rotated to: {FacingDirection}");
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