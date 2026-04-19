using Godot;
using System.Collections.Generic;

namespace Consensus.Nodes.UI;

public partial class MainMenu : CanvasLayer
{
    [Export] public PackedScene? LevelScene { get; set; }

    private Control MainButtons => GetNode<Control>("ButtonContainer");
    private Control LevelPanel => GetNode<Control>("LevelSelectPanel");
    private GridContainer LevelGrid => GetNode<GridContainer>("LevelSelectPanel/VBox/GridContainer");

    private Button StartButton => GetNode<Button>("ButtonContainer/StartButton");
    private Button QuitButton => GetNode<Button>("ButtonContainer/QuitButton");
    private Button BackButton => GetNode<Button>("LevelSelectPanel/VBox/BackButton");

    private readonly Dictionary<string, string> levels = new()
    {
        { "Sector 01", "res://Levels/level_1.tscn" },
        { "Sector 02", "res://Levels/level_2.tscn" },
        { "Sector 03", "res://Levels/level_3.tscn" },
        { "Sector 04", "res://Levels/level_4.tscn" }
    };

    public override void _Ready()
    {
        StartButton.Pressed += ToggleLevelSelect;
        QuitButton.Pressed += () => GetTree().Quit();
        BackButton.Pressed += ToggleLevelSelect;

        LevelPanel.Visible = false;

        foreach (var level in levels)
        {
            Button btn = new() { Text = level.Key };
            btn.Pressed += () => StartLevel(level.Value);
            LevelGrid.AddChild(btn);
        }
    }

    private void ToggleLevelSelect()
    {
        MainButtons.Visible = !MainButtons.Visible;
        LevelPanel.Visible = !LevelPanel.Visible;
    }

    private void StartLevel(string path)
    {
        var s = LevelScene?.Instantiate<LevelManager>();
        if (s == null)
        {
            GD.PrintErr($"[MainMenu] You should have LevelScene first!");
            return;
        }
        s.Name = "LevelRoot";
        GetTree().ChangeSceneToNode(s);
        s.LoadLevel(path);
    }
}