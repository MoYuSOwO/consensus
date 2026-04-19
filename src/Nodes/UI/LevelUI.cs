using Godot;
using System.Collections.Generic;
using Consensus.Models;
using Consensus.Models.Commands;
using Consensus.Utils;

namespace Consensus.Nodes.UI;

public partial class LevelUI : CanvasLayer
{
    [Export] public PackedScene? commandEntryScene;
	public PackedScene CommandEntryScene => BasicUtil.Must(commandEntryScene, "LevelUI");

	private LevelManager Root => (LevelManager)GetParent();

    private VBoxContainer ListContent => GetNode<VBoxContainer>("MainContainer/EditorPanel/VBox/CommandList/ListContent");
    private MenuButton AddButton => GetNode<MenuButton>("MainContainer/EditorPanel/VBox/AddButton");
	private CommandInspector Inspector => GetNode<CommandInspector>("MainContainer/EditorPanel/VBox/CommandInspector");
	private GameOverlay Overlay => GetNode<GameOverlay>("GameOverlay");
	private Button ExecuteButton => GetNode<Button>("MainContainer/EditorPanel/VBox/ExecuteButton");
	private Control EditorPanel => GetNode<Control>("MainContainer/EditorPanel");
    private Control ConsolePanel => GetNode<Control>("MainContainer/ConsolePanel");
    private RichTextLabel LogViewer => GetNode<RichTextLabel>("MainContainer/ConsolePanel/VBox/LogViewer");
    private Button RestartButton => GetNode<Button>("MainContainer/ConsolePanel/VBox/RestartButton");
	private Label StrengthLabel => GetNode<Label>("MainContainer/EditorPanel/VBox/StrengthLabel");
	private Button HideButton => GetNode<Button>("HideButton");
	private HBoxContainer MainContainer => GetNode<HBoxContainer>("MainContainer");

	private NetworkManager? _network;
	public NetworkManager Network => BasicUtil.Must(_network, "LevelUI");
	private TickManager? _ticker;
	public TickManager Ticker => BasicUtil.Must(_ticker, "LevelUI");
	private string? _mainRobotId;
	public string MainRobotId => BasicUtil.Must(_mainRobotId, "LevelUI");

    private int commandCounter = 0;

    public override void _Ready()
    {
        var popup = AddButton.GetPopup();
        popup.Clear();

        var names = CommandFactory.Names;
        for (int i = 0; i < names.Length; i++)
        {
            popup.AddItem($"+ {names[i]}", i); 
        }

		ConsolePanel.Visible = false;
        EditorPanel.Visible = true;

        popup.IdPressed += OnAddCommandPressed;
		Inspector.CommandSaved += OnCommandSaved;
		ExecuteButton.Pressed += OnExecutePressed;
		RestartButton.Pressed += Root.Reset;
		HideButton.Pressed += () => MainContainer.Visible = !MainContainer.Visible;

		Root.CurrentLevel.Win += () =>
		{
			var t = StrengthLabel.Text;
			var st = t.Split(": ");
			Overlay.OnGameEnded(true, $"Strength Use: {st[1]}, Time Use: {TickManager.TickToSecond(Ticker.CurrentTick)}s");
		};
		Root.CurrentLevel.Fail += () => Overlay.OnGameEnded(false);

		UpdateStrengthDisplay();
	}

	public void Init(NetworkManager network, TickManager ticker, string mainRobotId)
	{
		_network = network;
		_ticker = ticker;
		_mainRobotId = mainRobotId;
	}

	private void ShowError(string message)
	{
		AcceptDialog dialog = new()
        {
			Title = "SYSTEM ERROR",
			DialogText = message,
			Exclusive = true,
		};
		
		AddChild(dialog);
		dialog.PopupCentered();
		
		dialog.Confirmed += dialog.QueueFree;
	}

    private void OnAddCommandPressed(long id)
    {
        Command newCmd = CommandFactory.GetCommandFactory(CommandFactory.Names[(int)id]).Invoke();
        Inspector.EditCommand(newCmd, Network.RobotIds, null);
    }

	private void OnCommandSaved(Command cmd, CommandEntry? linkedUI)
    {
        if (linkedUI == null)
        {
            CommandEntry entryUI = CommandEntryScene.Instantiate<CommandEntry>();
			commandCounter = ListContent.GetChildCount();
            ListContent.AddChild(entryUI);
            
            entryUI.RequestEdit += OnRequestEdit;
            entryUI.Init(cmd, commandCounter, MainRobotId, Network, UpdateStrengthDisplay);
			UpdateStrengthDisplay();
        }
        else
        {
			UpdateStrengthDisplay();
            linkedUI.RefreshDisplay(); 
        }
    }

    private void OnRequestEdit(Command cmd, CommandEntry entryUI)
    {
        Inspector.EditCommand(cmd, Network.RobotIds, entryUI);
    }

	private void OnExecutePressed()
	{
		List<Command> allCommands = [];

		foreach (var child in ListContent.GetChildren())
		{
			if (child is CommandEntry entry)
			{
				allCommands.Add(entry.Cmd);
			}
		}

		HashSet<int> wrappedIndices = [];

		for (int i = 0; i < allCommands.Count; i++)
		{
			var cmd = allCommands[i];
			cmd.FromRobotId = MainRobotId;
			
			if (cmd is RelayCommand relay)
			{
				int targetIdx = relay.TargetIndex;

				if (targetIdx <= 0 || targetIdx > allCommands.Count)
				{
					ShowError($"The target #{targetIdx} of the relay instruction #{i + 1} does not exist!");
					return;
				}

				if (targetIdx - 1 == i)
				{
					ShowError($"Relay instruction #{i + 1} cannot point to itself!");
					return;
				}

				relay.Cmd = allCommands[targetIdx - 1];
				wrappedIndices.Add(targetIdx - 1);
			}
		}

		List<Command> rootCommands = [];
		for (int i = 0; i < allCommands.Count; i++)
		{
			if (!wrappedIndices.Contains(i))
			{
				rootCommands.Add(allCommands[i]);
			}
		}

		GD.Print("Starting to execute...");

		EditorPanel.Visible = false;
        ConsolePanel.Visible = true;

		Network.SendAll([.. rootCommands]); 
		Ticker.StartTicking();
	}

	public void PrintLog(string text, string hexColor = "#ffffff")
    {
        LogViewer.AppendText($"> [color={hexColor}]{text}[/color]\n");
    }

	public void UpdateStrengthDisplay()
	{
		float currentStrength = 0;
		
		foreach (var child in ListContent.GetChildren())
		{
			if (child is CommandEntry entry)
			{
				currentStrength += entry.Cmd.SendStrength; 
			}
		}

		float MaxStrength = Root.CurrentLevel.TotalSignalEnergy;

		StrengthLabel.Text = $"Strength: {currentStrength} / {MaxStrength}";

		if (currentStrength > MaxStrength)
		{
			StrengthLabel.LabelSettings.FontColor = Colors.Red;
			ExecuteButton.Disabled = true;
			ExecuteButton.Text = "[ SYSTEM OVERLOAD ]";
		}
		else
		{
			StrengthLabel.LabelSettings.FontColor = Colors.LimeGreen;
			ExecuteButton.Disabled = false;
			ExecuteButton.Text = "Execute";
		}
	}
}
