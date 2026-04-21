extends Command
class_name RotateCommand

static var ROTATE_TICK: int = 2

var direction: Util.Direction = Util.Direction.DOWN

func execute(robot: Node2D):
	robot.rotate_to(direction)
	
func set_param(_key: String, _value: Variant):
	if _key == "direction":
		direction = _value
		
func get_extra_params() -> Array[CommandParam]:
	return [
		CommandParam.new("direction", Util.TYPE_DIRECTION, direction)
	]
	
func extra_params_to_string() -> Dictionary[String, String]:
	return {
		"direction": Util.direction_to_string(direction)
	}
