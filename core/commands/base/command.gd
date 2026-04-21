@abstract
extends RefCounted
class_name Command

var from_robot_id: String = ""
var to_robot_id: String = ""
var send_tick: int = 0
var send_strength: float = 0.0

var calculated_arrival_tick: int = 0
var calculated_arrival_strength: float = 0.0
var calculated_arrival_loss_ratio: float = 0.0

@abstract
func execute(robot: Node2D)

func set_param(_key: String, _value: Variant):
	pass

func get_extra_params() -> Array[CommandParam]:
	return []

func extra_params_to_string() -> Dictionary[String, String]:
	return {}
	
func _to_string() -> String:
	var d = extra_params_to_string()
	var action = CommandFactory.get_command_name(get_script())
	
	var param_strings = []
	for key in d:
		param_strings.append("%s: %s" % [key, str(d[key])])
	
	var extra_params_str = " | ".join(param_strings)
	return "from: @%s, to: @%s, action: %s, params: %s" % [from_robot_id, to_robot_id, action, extra_params_str]
