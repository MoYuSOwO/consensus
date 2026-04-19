using Consensus.Utils;
using Godot;
using System;

namespace Consensus.Nodes;

public partial class LevelManager : Node
{
	[Export] public string LevelPath { get; set; } = "res://Levels/Level_test.tscn";

    private static string Caller => $"LevelManager";

	private TickManager? _ticker;
    public TickManager Ticker => BasicUtil.Must(_ticker, Caller);
    private NetworkManager? _network;
    public NetworkManager Network => BasicUtil.Must(_network, Caller);
    private Node? _currentLevel;
    public Node CurrentLevel => BasicUtil.Must(_currentLevel, Caller);

	public void LoadLevel(string levelPath)
    {
        GD.Print($"[LevelManager] Start to load level: {levelPath}");

        _ticker?.QueueFree();
        _network?.QueueFree();
        _currentLevel?.QueueFree();

        _ticker = null;
        _network = null;
        _currentLevel = null;

        // 等待当前帧结束，确保旧节点移除
        CallDeferred(nameof(InstantiateLevel), levelPath);
    }

	private void InstantiateLevel(string levelPath)
    {
        _ticker = new TickManager { Name = "TickManager" };
        AddChild(_ticker);

        _network = new NetworkManager { Name = "NetworkManager" };
        AddChild(_network);

        Network.Init(Ticker);

        var levelScene = GD.Load<PackedScene>(levelPath);
        if (levelScene == null)
        {
            GD.PrintErr($"[LevelManager] Fail to load level！Can not find file: {levelPath}");
            return;
        }

        _currentLevel = levelScene.Instantiate();
        _currentLevel.Name = "CurrentLevel";
        AddChild(_currentLevel);

        GD.Print($"[LevelManager] Sucessfully load level from {levelPath}!");
    }

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadLevel(LevelPath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
