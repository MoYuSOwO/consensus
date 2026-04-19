using Consensus.Models;
using Consensus.Models.Commands;
using Consensus.Nodes;
using Consensus.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Text;

namespace Consensus.Nodes.UI;

public partial class CommandEntry : PanelContainer
{
	private Command? _cmd;
	public Command Cmd => BasicUtil.Must(_cmd, "CommandEntry");
	private int? _index = null;
	public int Index => BasicUtil.Must(_index, "CommandEntry");

    private Label IndexCol => GetNode<Label>("HBox/HBox/IndexCol");
    private Label SendTimeCol => GetNode<Label>("HBox/HBox/SendTimeCol");
	private Label ArriveTimeCol => GetNode<Label>("HBox/HBox/ArriveTimeCol");
	private Label ActionCol => GetNode<Label>("HBox/HBox/ActionCol");
	private Label TargetCol => GetNode<Label>("HBox/HBox/TargetCol");
	private Label SendStrengthCol => GetNode<Label>("HBox/HBox/SendStrengthCol");
	private Label ArriveStrengthCol => GetNode<Label>("HBox/HBox/ArriveStrengthCol");
	private Label LossProbCol => GetNode<Label>("HBox/HBox/LossProbCol");
	private Label ExtraParamsCol => GetNode<Label>("HBox/HBox/ExtraParamsCol");
    private Button DelButton => GetNode<Button>("HBox/DelButton");
	private Button EditButton => GetNode<Button>("HBox/EditButton");

	private NetworkManager? _network;
	public NetworkManager Network => BasicUtil.Must(_network, "CommandEntry");

	private Action _action = () => {};

	public event Action<Command, CommandEntry>? RequestEdit;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		DelButton.Pressed += OnDelete;
		EditButton.Pressed += () => RequestEdit?.Invoke(Cmd, this);
	}

	public void Init(Command cmd, int index, string mainRobotId, NetworkManager network, Action action)
    {
		_network = network;
		_action = action;
        _cmd = cmd;
		_cmd.FromRobotId = mainRobotId;
        _index = index;
		RefreshDisplay();
    }

	public void RefreshDisplay()
    {
        var d = Cmd.ExtraParamsToString();

		Robot from = Network.Registry[Cmd.FromRobotId], to = Network.Registry[Cmd.ToRobotId];
		var v = AlgorithmUtil.CalculateInf(from, to, Cmd);

        IndexCol.Text = $"#{Index + 1}";
        SendTimeCol.Text = $"T+{TickManager.TickToSecond(Cmd.SendTick)}s";
		ArriveTimeCol.Text = $"T+({TickManager.TickToSecond(Cmd.SendTick + v.DelayTicks.Min)}s - {TickManager.TickToSecond(Cmd.SendTick + v.DelayTicks.Max)}s)";
        ActionCol.Text = CommandFactory.GetCommandName(Cmd.GetType());
        TargetCol.Text = $"@{Cmd.ToRobotId}";
        SendStrengthCol.Text = $"{Cmd.SendStrength}";
		ArriveStrengthCol.Text = v.Strength.IsNumber ? $"{v.Strength.Value:F1}" : $"{v.Strength.Min:F1} - {v.Strength.Max:F1}";
		LossProbCol.Text = v.LossProb.IsNumber ? $"{v.LossProb.Value * 100:F1}%" : $"{v.LossProb.Min * 100:F1}% - {v.LossProb.Max * 100:F1}%";
        
        var paramStrings = new List<string>();
		foreach (var e in d)
		{
			paramStrings.Add($"{e.Key}: {e.Value}");
		}
		ExtraParamsCol.Text = string.Join("\n", paramStrings) + "   ";
    }

	private void OnDelete()
	{
		GetParent().RemoveChild(this);
		QueueFree();
		_action.Invoke();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
