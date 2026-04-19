using Consensus.Models.Commands;
using Consensus.Models.Entities;
using Consensus.Nodes;
using Consensus.Nodes.UI;
using Consensus.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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

    private static readonly Texture2D PlayerTex = GD.Load<Texture2D>("res://Assets/player.png");
    private static readonly Texture2D DefaultTex = GD.Load<Texture2D>("res://Assets/robot.png");
    
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

    private Tween? progressTween;

    private readonly Dictionary<Vector2I, GridEntity> Entities = [];

    public Sprite2D Body => GetNode<Sprite2D>("Body");
    public ColorRect[] ColorRects => [.. GetNode("HBoxContainer").GetChildren().OfType<ColorRect>()];
    public ProgressBar Progress => GetNode<ProgressBar>("ProgressBar");
    public CpuParticles2D GreenParticle => GetNode<CpuParticles2D>("GreenParticle");
    public CpuParticles2D RedParticle => GetNode<CpuParticles2D>("RedParticle");
    public CpuParticles2D BlueParticle => GetNode<CpuParticles2D>("BlueParticle");
    public Label NameLabel => GetNode<Label>("ColorRect/CenterContainer/NameLabel");

    public override void _Ready()
    {
        NameLabel.Text = RobotId;
    }

    public void Init(TileMapLayer map, NetworkManager network, TickManager ticker, IDictionary<Vector2I, GridEntity> entities, bool isPlayer = false)
    {
        _map = map;
        _network = network;
        _ticker = ticker;

        foreach (var entity in entities) Entities.Add(entity.Key, entity.Value);

        GridPos = MapLayer.LocalToMap(Position);
        Position = MapLayer.MapToLocal(GridPos);

        Network.RegisterRobot(this);
        Ticker.TickUpdate += OnTickUpdate;

        if (isPlayer) Body.Texture = PlayerTex;
        else Body.Texture = DefaultTex;
    }

    public void ReceiveCommand(Command cmd, LevelUI ui)
    {
        if (actionQueue.Count < 3)
        {
            actionQueue.Enqueue(cmd);
            BlueParticle.Emitting = true;
            ui.PrintLog($"[T+{TickManager.TickToSecond(Ticker.CurrentTick)}] command recieved: {cmd}. Queue length: {actionQueue.Count}", "#7ee787");
        }
        else
        {
            RedParticle.Emitting = true;
            ui.PrintLog($"[T+{TickManager.TickToSecond(Ticker.CurrentTick)}] command package lost due to full queue: {cmd}", "#ff7b72");
        }
        UpdateQueueLEDs();
    }

    private void OnTickUpdate(int currentTick)
    {
        if (!IsBusy && actionQueue.Count > 0)
        {
            IsBusy = true;
            var cmd = actionQueue.Dequeue();
            UpdateQueueLEDs();
            cmd.Execute(this);
        }
    }

    private bool IsCellWall(Vector2I gridPos)
    {
        TileData tileData = MapLayer.GetCellTileData(gridPos);
        Entities.TryGetValue(gridPos, out GridEntity? entity);
        return (tileData != null && tileData.GetCustomData("is_wall").AsBool()) || (entity != null && !entity.IsWalkable);
    }

    public void Step(int length)
    {
        IsBusy = true;

        Vector2I direction = FacingDirection.ToVector2I();
        int actualSteps = 0;

        for (int i = 1; i <= length; i++)
        {
            Vector2I nextGrid = GridPos + direction * i;
            if (IsCellWall(nextGrid)) break; 
            actualSteps = i;
        }

        float basicDuration = TickManager.TickToSecond(StepCommand.TickPerStep);
        float moveDuration = Mathf.Max(actualSteps * basicDuration, basicDuration);

        StartProgressBar(moveDuration);

        var tween = GetTree().CreateTween();

        if (actualSteps == 0)
        {
            Vector2 currentPos = Position;
            Vector2 bumpPos = currentPos + (Vector2)direction * 8f;
            
            tween.TweenProperty(this, "position", bumpPos, basicDuration * 0.3f).SetTrans(Tween.TransitionType.Quad);
            tween.TweenProperty(this, "position", currentPos, basicDuration * 0.7f).SetTrans(Tween.TransitionType.Bounce);
        }
        else
        {
            Vector2I tempGrid = GridPos;
            for (int i = 0; i < actualSteps; i++)
            {
                tempGrid += direction;
                Vector2 targetPixel = MapLayer.MapToLocal(tempGrid);
                
                tween.TweenProperty(this, "position", targetPixel, basicDuration);
                
                Vector2I stepGrid = tempGrid;
                tween.TweenCallback(Callable.From(() => {
                    UpdateGridPosition(stepGrid);
                }));
            }
        }

        tween.Finished += () => {
            IsBusy = false;
            if (actualSteps < length) GD.Print($"[Robot - {RobotId}] robot is blocked! stop walking...");
        };
    }

    private void UpdateGridPosition(Vector2I newGridPos)
    {
        if (Entities.TryGetValue(GridPos, out GridEntity? entity1))
        {
            if (entity1 != null && entity1 is GridInteractable interactable)
            {
                interactable.OnExit();
            }
        }
        GridPos = newGridPos;
        if (Entities.TryGetValue(GridPos, out GridEntity? entity2))
        {
            if (entity2 != null && entity2 is GridInteractable interactable)
            {
                interactable.OnEnter();
            }
        }
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

        StartProgressBar(duration);

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
        StartProgressBar(time);
        var timer = GetTree().CreateTimer(time);
        timer.Timeout += () => {
            IsBusy = false;
        };
        return timer;
    }

    private void StartProgressBar(float duration)
    {
        progressTween?.Kill();

        Progress.Value = 0.0; 
        Progress.Visible = true;

        progressTween = GetTree().CreateTween();
        
        progressTween.TweenProperty(Progress, "value", 100.0, (double)duration);
        
        progressTween.Finished += () => {
            Progress.Value = 0.0;
        };
    }

    private void UpdateQueueLEDs()
    {
        ColorRects[0].Color = actionQueue.Count > 2 ? Colors.Red : Colors.Green;
        ColorRects[1].Color = actionQueue.Count > 1 ? Colors.Red : Colors.Green;
        ColorRects[2].Color = actionQueue.Count > 0 ? Colors.Red : Colors.Green;
    }
}