using Consensus.Models;
using Consensus.Nodes.UI;
using Consensus.Utils;
using Godot;
using System;

namespace Consensus.Nodes;

public partial class LevelManager : Node
{
	public string LevelPath { get; set; } = "res://Levels/Level_test.tscn";
    [Export] public string UIPath { get; set; } = "res://Levels/level_ui.tscn";

    private static string Caller => $"LevelManager";

	private TickManager? _ticker;
    public TickManager Ticker => BasicUtil.Must(_ticker, Caller);
    private NetworkManager? _network;
    public NetworkManager Network => BasicUtil.Must(_network, Caller);
    private LevelBase? _currentLevel;
    public LevelBase CurrentLevel => BasicUtil.Must(_currentLevel, Caller);
    private LevelUI? _currentUI;
    public LevelUI CurrentUI => BasicUtil.Must(_currentUI, Caller);

	public void LoadLevel(string levelPath)
    {
        GD.Print($"[LevelManager] Start to load level: {levelPath}");

        LevelPath = levelPath;

        if (_ticker != null) { RemoveChild(_ticker); _ticker.QueueFree(); }
        if (_network != null) { RemoveChild(_network); _network.QueueFree(); }
        if (_currentLevel != null) { RemoveChild(_currentLevel); _currentLevel.QueueFree(); }
        if (_currentUI != null) { RemoveChild(_currentUI); _currentUI.QueueFree(); }

        _ticker = null;
        _network = null;
        _currentLevel = null;
        _currentUI = null;

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
            GD.PrintErr($"[LevelManager] Fail to load level! Can not find file: {levelPath}");
            return;
        }
        _currentLevel = levelScene.Instantiate<LevelBase>();
        if (_currentLevel == null)
        {
            GD.PrintErr($"[LevelManager] Root node of {levelPath} does not have a script attached that inherits from LevelBase!");
            return;
        }
        _currentLevel.Name = "CurrentLevel";
        AddChild(_currentLevel);

        var UIScene = GD.Load<PackedScene>(UIPath);
        if (UIScene == null)
        {
            GD.PrintErr($"[LevelManager] Fail to load UI! Can not find file: {UIPath}");
            return;
        }
        _currentUI = UIScene.Instantiate<LevelUI>();
        if (_currentUI == null)
        {
            GD.PrintErr($"[LevelManager] Root node of {UIPath} does not have a script attached that inherits from LevelUI!");
            return;
        }
        _currentUI.Name = "LevelUI";
        _currentUI.Init(Network, Ticker, _currentLevel.MainRobotId);
        AddChild(_currentUI);

        GD.Print($"[LevelManager] Sucessfully load level from {levelPath} and UI from {UIPath}!");
    }

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

    public void Reset()
    {
        LoadLevel(LevelPath);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
