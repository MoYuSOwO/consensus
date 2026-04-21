extends Command
class_name StepCommand

static var TICK_PER_STEP: int = 2

var length: int

func execute(robot: Node2D):
	robot.step(length)
	
func set_param(_key: String, _value: Variant):
	if _key == "length":
		length = _value
		
func get_extra_params() -> Array[CommandParam]:
	return [
		CommandParam.new("length", TYPE_INT, length)
	]
	
func extra_params_to_string() -> Dictionary[String, String]:
	return {
		"length": String.num_int64(length)
	}
