extends PanelContainer
class_name CommandInspector

signal command_saved(cmd: Command, entry: CommandEntry)

@onready var send_time: SpinBox = $VBox/BasicParamsContainer/SendTimeContainer/SendTime
@onready var target_robot: OptionButton = $VBox/BasicParamsContainer/TargetContainer/TargetRobot
@onready var strength: SpinBox = $VBox/BasicParamsContainer/StrengthContainer/Strength
@onready var extra_container: VBoxContainer = $VBox/ExtraParamsContainer
@onready var save_button: Button = $VBox/SaveButton

var editing_cmd: Command
var linked_entry_ui: CommandEntry

func _ready() -> void:
	save_button.pressed.connect(_on_save_pressed)
	hide()

func edit_command(cmd: Command, available_robots: Array[String], entry_ui: CommandEntry = null) -> void:
	editing_cmd = cmd
	linked_entry_ui = entry_ui
	show()

	send_time.value = TickManager.tick_to_second(cmd.send_tick)
	strength.value = cmd.send_strength

	target_robot.clear()
	for i in range(available_robots.size()):
		target_robot.add_item(available_robots[i])
		if available_robots[i] == cmd.to_robot_id:
			target_robot.select(i)

	for child in extra_container.get_children():
		child.queue_free()

	var extra_params = cmd.get_extra_params()
	for param in extra_params:
		var row = HBoxContainer.new()
		var label = Label.new()
		label.text = param.name
		label.custom_minimum_size = Vector2(80, 0)
		row.add_child(label)

		if param.data_type == TYPE_INT:
			var input = SpinBox.new()
			input.value = param.value
			input.min_value = 0
			input.max_value = 999
			input.value_changed.connect(func(val): editing_cmd.set_param(param.name, int(val)))
			row.add_child(input)
			
		elif param.data_type == Util.TYPE_DIRECTION:
			var opt = OptionButton.new()
			opt.add_item("Up", 0)
			opt.add_item("Down", 1)
			opt.add_item("Left", 2)
			opt.add_item("Right", 3)
			opt.select(param.value)
			opt.item_selected.connect(func(id): editing_cmd.set_param(param.name, Util.value_to_direction(id)))
			row.add_child(opt)

		extra_container.add_child(row)

func _on_save_pressed() -> void:
	editing_cmd.send_tick = TickManager.second_to_tick(send_time.value)
	editing_cmd.send_strength = float(strength.value)
	
	if target_robot.item_count > 0 and target_robot.selected >= 0:
		editing_cmd.to_robot_id = target_robot.get_item_text(target_robot.selected)

	hide()
	command_saved.emit(editing_cmd, linked_entry_ui)
