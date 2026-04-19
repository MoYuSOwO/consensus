using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Consensus.Models.Entities;
using Consensus.Nodes;
using Godot;

namespace Consensus.Models;

public abstract partial class LevelBase : Node
{
    [Export] public float TotalSignalEnergy = 100.0f;
    [Export] public string MainRobotId { get; set; } = "";

    private const int FailWaitTick = 20;
    private int failWait = 0;

    protected TickManager Ticker => GetParent().GetNode<TickManager>("TickManager");
    protected NetworkManager Network => GetParent().GetNode<NetworkManager>("NetworkManager");
    protected TileMapLayer MapLayer => GetNode<TileMapLayer>("TileMapLayer");
    protected Camera2D Camera => GetNode<Camera2D>("Camera2D");
    protected ImmutableDictionary<string, Robot> Robots => GetNode<Node>("Robots").GetChildren().OfType<Robot>().ToImmutableDictionary(k => k.RobotId, v => v);
    protected ImmutableArray<GridEntity> Entities => [.. GetNode<Node>("Entities").GetChildren().OfType<GridEntity>()];


    [Signal]
    public delegate void WinEventHandler();

    [Signal]
    public delegate void FailEventHandler();

    public override void _Ready()
    {
        Ticker.TickUpdate += OnTickStart;
        foreach (var entity in Entities)
        {
            entity.Init(MapLayer);
        }
        var d = Entities.ToImmutableDictionary(k => k.GridPos, v => v);
        foreach (var robot in Robots.Values)
        {
            robot.Init(MapLayer, Network, Ticker, d, robot.Name == MainRobotId);
        }
        InitLevel();
    }

    protected virtual void InitLevel() {}
    protected virtual void OnTick(int tick) {}
    protected abstract bool IsGoalMet();

    private void OnTickStart(int tick)
    {
        CheckStatus();
        OnTick(tick);
    }

    public void CheckStatus()
    {
        if (IsGoalMet()) EmitSignal(SignalName.Win);
        else if (IsSystemIdle()) EmitSignal(SignalName.Fail);
    }

    private bool IsSystemIdle()
    {
        return Network.IsFlightEmpty && Network.Registry.Values.All(robot => robot.IsQueueEmpty) && failWait++ > FailWaitTick;
    }
}