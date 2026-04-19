using Godot;
using System;

namespace Consensus.Nodes.UI;

public partial class GameOverlay : CanvasLayer
{
    private Control BgDim => GetNode<Control>("BackgroundDim");
    private Label TitleLabel => GetNode<Label>("BackgroundDim/CenterContainer/VBox/TitleLabel");
	private Label ScoreLabel => GetNode<Label>("BackgroundDim/CenterContainer/VBox/ScoreLabel");
	private Button PauseButton => GetNode<Button>("PauseButton");
    private Button ContinueBtn => GetNode<Button>("BackgroundDim/CenterContainer/VBox/ContinueBtn");
	private Button RetryBtn => GetNode<Button>("BackgroundDim/CenterContainer/VBox/RetryBtn");
	private Button MenuBtn => GetNode<Button>("BackgroundDim/CenterContainer/VBox/MenuBtn");
	private LevelManager Level => (LevelManager)GetParent().GetParent();

    public override void _Ready()
    {
        BgDim.Visible = false;
		
		PauseButton.Pressed += Pause;
        ContinueBtn.Pressed += Unpause;
        RetryBtn.Pressed += () =>
		{
			GetTree().Paused = false;
			Level.Reset();
		};
        MenuBtn.Pressed += () =>
		{
			GetTree().Paused = false;
			GetTree().ChangeSceneToFile("res://Levels/main_menu.tscn");
		};
    }

    public void Pause()
    {
        GetTree().Paused = true;
        TitleLabel.Text = "GAME PAUSED";
        ContinueBtn.Visible = true;
        BgDim.Visible = true;
    }

    private void Unpause()
    {
        GetTree().Paused = false;
        BgDim.Visible = false;
    }

    public void OnGameEnded(bool win, string s = "")
    {
        GetTree().Paused = true;
        TitleLabel.Text = win ? "MISSION ACCOMPLISHED" : "CONNECTION LOST";
		ScoreLabel.Text = s;
        ContinueBtn.Visible = false;
        BgDim.Visible = true;
        
        TitleLabel.SelfModulate = win ? Colors.LimeGreen : Colors.Red;
    }
}