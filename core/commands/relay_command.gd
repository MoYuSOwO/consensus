extends Command
class_name RelayCommand

static var RELAY_TICK: int = 2

var target_index : int = 0
var command: Command

func execute(robot: Node2D):
	var timer: SceneTreeTimer = robot.wait(TickManager.tick_to_second(RELAY_TICK))
	timer.timeout.connect(
		func():
			command.from_robot_id = robot.id
			command.send_tick = robot.ticker.get_current_tick()
			command.send_strength = send_strength
			robot.network.send(command)
	)
	
func set_param(_key: String, _value: Variant):
	if _key == "command":
		command = _value
	elif _key == "target_index":
		target_index = _value
		
func get_extra_params() -> Array[CommandParam]:
	return [
		CommandParam.new("target_index", TYPE_INT, target_index)
	]
	
func extra_params_to_string() -> Dictionary[String, String]:
	return {
		"target_index": String.num_int64(target_index)
	}
