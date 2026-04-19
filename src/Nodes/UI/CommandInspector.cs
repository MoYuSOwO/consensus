using Godot;
using Consensus.Models.Commands;
using System;
using System.Collections.Generic;
using Consensus.Utils;
using System.Collections.Immutable;

namespace Consensus.Nodes.UI;

public partial class CommandInspector : PanelContainer
{
    public event Action<Command, CommandEntry?>? CommandSaved;

    public SpinBox SendTime => GetNode<SpinBox>("VBox/BasicParamsContainer/SendTimeContainer/SendTime");
    public OptionButton TargetRobot => GetNode<OptionButton>("VBox/BasicParamsContainer/TargetContainer/TargetRobot");
    public SpinBox Strength => GetNode<SpinBox>("VBox/BasicParamsContainer/StrengthContainer/Strength");
    public VBoxContainer ExtraContainer => GetNode<VBoxContainer>("VBox/ExtraParamsContainer");
    public Button SaveButton => GetNode<Button>("VBox/SaveButton");

    private Command editingCmd = Command.Dummy;
    private CommandEntry? linkedEntryUI;

    public override void _Ready()
    {
        SaveButton.Pressed += OnSavePressed;
        Visible = false;
    }

    public void EditCommand(Command cmd, ImmutableArray<string> availableRobots, CommandEntry? entryUI = null)
    {
        editingCmd = cmd;
        linkedEntryUI = entryUI;
        Visible = true;

        SendTime.Value = TickManager.TickToSecond(cmd.SendTick);
        Strength.Value = cmd.SendStrength;

        TargetRobot.Clear();
        foreach (var robotId in availableRobots)
        {
            TargetRobot.AddItem(robotId);
        }
        
        if (!string.IsNullOrEmpty(cmd.ToRobotId))
        {
            for (int i = 0; i < TargetRobot.ItemCount; i++) 
            {
                if (TargetRobot.GetItemText(i) == cmd.ToRobotId) 
                {
                    TargetRobot.Select(i);
                    break;
                }
            }
        }

        foreach (var child in ExtraContainer.GetChildren()) 
        {
            child.QueueFree();
        }

        var extraParams = cmd.GetExtraParams(); 
        
        foreach (var param in extraParams)
        {
            var row = new HBoxContainer();
            row.AddChild(new Label { Text = param.Name, CustomMinimumSize = new Vector2(80, 0) });

            if (param.DataType == typeof(int))
            {
                var input = new SpinBox { 
                    Value = Convert.ToDouble(param.Value),
                    MinValue = 0,
                    MaxValue = 999 
                };
                input.ValueChanged += (val) => editingCmd.SetParam(param.Name, (int)val); 
                row.AddChild(input);
            }
            else if (param.DataType == typeof(Direction))
            {
                var opt = new OptionButton();
                foreach (var name in Enum.GetNames(typeof(Direction))) 
                {
                    opt.AddItem(name);
                }
                opt.Select((int)param.Value);
                opt.ItemSelected += (id) => editingCmd.SetParam(param.Name, (Direction)id);
                row.AddChild(opt);
            }

            ExtraContainer.AddChild(row);
        }
    }

    private void OnSavePressed()
    {
        editingCmd.SendTick = TickManager.SecondToTick(SendTime.Value);
        editingCmd.SendStrength = (float)Strength.Value;
        
        if (TargetRobot.ItemCount > 0 && TargetRobot.Selected >= 0) {
            editingCmd.ToRobotId = TargetRobot.GetItemText(TargetRobot.Selected);
        }

        Visible = false;

        CommandSaved?.Invoke(editingCmd, linkedEntryUI);
    }
}