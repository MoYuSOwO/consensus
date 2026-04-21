extends PanelContainer
class_name CommandEntry

signal request_edit(cmd: Command, entry: CommandEntry)
signal entry_deleted

var cmd: Command
var index: int

var network: NetworkManager

@onready var index_col: Label = $HBox/HBox/IndexCol
@onready var send_time_col: Label = $HBox/HBox/SendTimeCol
@onready var arrive_time_col: Label = $HBox/HBox/ArriveTimeCol
@onready var action_col: Label = $HBox/HBox/ActionCol
@onready var target_col: Label = $HBox/HBox/TargetCol
@onready var send_strength_col: Label = $HBox/HBox/SendStrengthCol
@onready var arrive_strength_col: Label = $HBox/HBox/ArriveStrengthCol
@onready var loss_prob_col: Label = $HBox/HBox/LossProbCol
@onready var extra_params_col: Label = $HBox/HBox/ExtraParamsCol

@onready var del_button: Button = $HBox/DelButton
@onready var edit_button: Button = $HBox/EditButton

func _ready() -> void:
	del_button.pressed.connect(_on_delete)
	edit_button.pressed.connect(func(): request_edit.emit(cmd, self))

func init(_cmd: Command, _index: int, main_robot_id: String, _network: NetworkManager) -> void:
	network = _network
	cmd = _cmd
	cmd.from_robot_id = main_robot_id
	index = _index
	refresh_display()

func refresh_display() -> void:
	var from_robot = network.get_robot_by_id(cmd.from_robot_id)
	var to_robot = network.get_robot_by_id(cmd.to_robot_id)
	var v = Util.calculate_inf(from_robot, to_robot, cmd)

	index_col.text = "#%d" % (index + 1)
	send_time_col.text = "T+%ss" % TickManager.tick_to_second(cmd.send_tick)
	
	arrive_time_col.text = "T+(%ss - %ss)" % [TickManager.tick_to_second(cmd.send_tick + v.delay_ticks.min), TickManager.tick_to_second(cmd.send_tick + v.delay_ticks.max)]
	
	action_col.text = CommandFactory.get_command_name(cmd.get_script())
	target_col.text = "@%s" % cmd.to_robot_id
	send_strength_col.text = str(cmd.send_strength)
	
	arrive_strength_col.text = "%.1f" % v.strength.value if v.strength.is_number() else "%.1f - %.1f" % [v.strength.min, v.strength.max]
	loss_prob_col.text = "%.1f%%" % (v.loss_prob.value * 100) if v.loss_prob.is_number() else "%.1f%% - %.1f%%" % [v.loss_prob.min * 100, v.loss_prob.max * 100] if v.loss_prob.is_valid() else "100%"
	
	var param_strings = []
	var extra_params_dict = cmd.extra_params_to_string()
	for key in extra_params_dict:
		param_strings.append("%s: %s" % [key, extra_params_dict[key]])
	
	extra_params_col.text = "\n".join(param_strings) + "   "

func _on_delete() -> void:
	entry_deleted.emit()
	get_parent().remove_child(self)
	queue_free()
