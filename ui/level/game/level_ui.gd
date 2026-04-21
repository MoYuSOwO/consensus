extends CanvasLayer
class_name LevelUI

@export var command_entry_scene: PackedScene

@onready var root: LevelManager = get_parent()
@onready var list_content: VBoxContainer = $MainContainer/EditorPanel/VBox/CommandList/ListContent
@onready var add_button: MenuButton = $MainContainer/EditorPanel/VBox/AddButton
@onready var inspector: CommandInspector = $MainContainer/EditorPanel/VBox/CommandInspector
@onready var overlay: GameOverlay = $GameOverlay
@onready var execute_button: Button = $MainContainer/EditorPanel/VBox/ExecuteButton
@onready var editor_panel: Control = $MainContainer/EditorPanel
@onready var console_panel: Control = $MainContainer/ConsolePanel
@onready var log_viewer: RichTextLabel = $MainContainer/ConsolePanel/VBox/LogViewer
@onready var restart_button: Button = $MainContainer/ConsolePanel/VBox/RestartButton
@onready var strength_label: Label = $MainContainer/EditorPanel/VBox/StrengthLabel
@onready var hide_button: Button = $HideButton
@onready var main_container: HBoxContainer = $MainContainer

var network: NetworkManager
var ticker: TickManager
var main_robot_id: String
var command_counter: int = 0

func _ready() -> void:
	var popup = add_button.get_popup()
	popup.clear()

	var names = CommandFactory.get_all_names()
	for i in range(names.size()):
		popup.add_item("+ " + names[i], i)
		
	console_panel.hide()
	editor_panel.show()

	popup.id_pressed.connect(_on_add_command_pressed)
	inspector.command_saved.connect(_on_command_saved)
	execute_button.pressed.connect(_on_execute_pressed)
	restart_button.pressed.connect(root.reset)
	hide_button.pressed.connect(func(): main_container.visible = not main_container.visible)

	root.get_current_level().win.connect(_on_level_win)
	root.get_current_level().fail.connect(func(): overlay.on_game_ended(false))

	update_strength_display()

func init(_network: NetworkManager, _ticker: TickManager, _main_robot_id: String) -> void:
	network = _network
	ticker = _ticker
	main_robot_id = _main_robot_id

func _show_error(message: String) -> void:
	var dialog = AcceptDialog.new()
	dialog.title = "SYSTEM ERROR"
	dialog.dialog_text = message
	dialog.exclusive = true
	add_child(dialog)
	dialog.popup_centered()
	dialog.confirmed.connect(dialog.queue_free)

func _on_add_command_pressed(id: int) -> void:
	var names = CommandFactory.get_all_names()
	var new_cmd = CommandFactory.create_command(names[id])
	inspector.edit_command(new_cmd, network.get_robot_ids())

func _on_command_saved(cmd: Command, linked_ui: CommandEntry) -> void:
	if linked_ui == null:
		var entry_ui: CommandEntry = command_entry_scene.instantiate()
		command_counter = list_content.get_child_count()
		list_content.add_child(entry_ui)
		
		entry_ui.request_edit.connect(func(c, u): inspector.edit_command(c, network.get_robot_ids(), u))
		entry_ui.entry_deleted.connect(update_strength_display)
		
		entry_ui.init(cmd, command_counter, main_robot_id, network)
	else:
		linked_ui.refresh_display()
	
	update_strength_display()

func _on_execute_pressed() -> void:
	var all_commands: Array[Command] = []
	for child in list_content.get_children():
		if child is CommandEntry:
			all_commands.append(child.cmd)

	var wrapped_indices = {}

	for i in range(all_commands.size()):
		var cmd: Command = all_commands[i]
		cmd.from_robot_id = main_robot_id
		
		if cmd is RelayCommand: 
			var target_idx = cmd.target_index
			if target_idx <= 0 or target_idx > all_commands.size():
				_show_error("The target #%d of the relay instruction #%d does not exist!" % [target_idx, i + 1])
				return
			if target_idx - 1 == i:
				_show_error("Relay instruction #%d cannot point to itself!" % [i + 1])
				return
			
			cmd.command = all_commands[target_idx - 1]
			wrapped_indices[target_idx - 1] = true

	var root_commands: Array[Command] = []
	for i in range(all_commands.size()):
		if not wrapped_indices.has(i):
			root_commands.append(all_commands[i])

	print("Starting to execute...")
	editor_panel.hide()
	console_panel.show()

	network.send_all(root_commands)
	ticker.start_ticking()

func print_log(text: String, hex_color: String = "#ffffff") -> void:
	log_viewer.append_text("> [color=%s]%s[/color]\n" % [hex_color, text])

func update_strength_display() -> void:
	var current_strength: float = 0
	for child in list_content.get_children():
		if child is CommandEntry:
			current_strength += child.cmd.send_strength

	var max_strength = root.get_current_level().total_signal_energy
	strength_label.text = "Strength: %s / %s" % [current_strength, max_strength]

	if current_strength > max_strength:
		strength_label.label_settings.font_color = Color.RED
		execute_button.disabled = true
		execute_button.text = "[ SYSTEM OVERLOAD ]"
	else:
		strength_label.label_settings.font_color = Color.LIME_GREEN
		execute_button.disabled = false
		execute_button.text = "Execute"

func _on_level_win() -> void:
	var st = strength_label.text.split(": ")
	if st.size() > 1:
		overlay.on_game_ended(true, "Strength Use: %s, Time Use: %ss" % [st[1], TickManager.tick_to_second(ticker.get_current_tick())])
