using System.Collections.Immutable;
using System.Linq;
using Consensus.Nodes;
using Godot;

namespace Consensus.Models;

public abstract partial class LevelBase : Node
{
    [Export] public float TotalSignalEnergy = 100.0f;

    private float _energy;
    public float CurrentSignalEnergy
    {
        get => _energy;
        protected set => _energy = Mathf.Clamp(value, 0.0f, 1.0f);
    }

    protected TickManager Ticker => GetParent().GetNode<TickManager>("TickManager");
    protected NetworkManager Network => GetParent().GetNode<NetworkManager>("NetworkManager");
    protected TileMapLayer MapLayer => GetNode<TileMapLayer>("TileMapLayer");
    protected Camera2D Camera => GetNode<Camera2D>("Camera2D");
    protected ImmutableDictionary<string, Robot> Robots => GetNode<Node>("Robots").GetChildren().OfType<Robot>().ToImmutableDictionary(k => k.RobotId, v => v);

    [Signal]
    public delegate void WinEventHandler();

    [Signal]
    public delegate void FailEventHandler();

    public override void _Ready()
    {
        CurrentSignalEnergy = TotalSignalEnergy;
        Ticker.TickUpdate += OnTick;
        foreach (var robot in Robots.Values)
        {
            robot.Init(MapLayer, Network, Ticker);
        }
        InitLevel();
    }

    protected virtual void InitLevel() {}
    protected virtual void OnTick(int tick) {}
    protected abstract bool IsGoalMet();

    public void CheckStatus()
    {
        if (IsGoalMet()) EmitSignal(SignalName.Win);
        else if (CurrentSignalEnergy <= 0 && IsSystemIdle()) EmitSignal(SignalName.Fail);
    }

    private bool IsSystemIdle()
    {
        return Network.IsFlightEmpty && Network.Registry.Values.Any(robot => !robot.IsQueueEmpty);
    }
}