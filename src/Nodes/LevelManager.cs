using Godot;
using System;

namespace Consensus.Nodes;

public partial class LevelManager : Node
{
	private TickManager? _tickManager;
    private NetworkManager? _networkManager;
    private Node? _currentLevel;

	public void LoadLevel(string levelPath)
    {
        GD.Print($"[LevelManager] Start to load level: {levelPath}");

        _tickManager?.QueueFree();
        _networkManager?.QueueFree();
        _currentLevel?.QueueFree();

		foreach (var node in GetChildren()) node.QueueFree();

        // 等待当前帧结束，确保旧节点移除
        CallDeferred(nameof(InstantiateLevel), levelPath);
    }

	private void InstantiateLevel(string levelPath)
    {
        _tickManager = new TickManager { Name = "TickManager" };
        AddChild(_tickManager);

        _networkManager = new NetworkManager { Name = "NetworkManager" };
        AddChild(_networkManager);

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
		LoadLevel("res://Scenes/Levels/Level_test.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
