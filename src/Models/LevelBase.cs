using System.Linq;
using Consensus.Nodes;
using Godot;

namespace Consensus.Models;

public abstract partial class LevelBase : Node2D
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

    [Signal]
    public delegate void WinEventHandler();

    [Signal]
    public delegate void FailEventHandler();

    public override void _Ready()
    {
        CurrentSignalEnergy = TotalSignalEnergy;
        Ticker.TickUpdate += OnTick;
        InitLevel();
    }

    protected abstract void InitLevel();
    protected abstract void OnTick(int tick);
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