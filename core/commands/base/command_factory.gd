extends RefCounted
class_name CommandFactory

static var _factory_registry: Dictionary[String, Callable] = {
	"Step": func(): return StepCommand.new(),
	"Rotate": func(): return RotateCommand.new(),
	"Relay": func(): return RelayCommand.new(),
}

static var _name_registry: Dictionary = {}

static func _static_init():
	for key in _factory_registry:
		var cmd = _factory_registry[key].call()
		_name_registry[cmd.get_script()] = key

static func create_command(name: String) -> Command:
	if _factory_registry.has(name):
		return _factory_registry[name].call()
	return null

static func get_command_name(script_obj: Script) -> String:
	return _name_registry.get(script_obj, "Unknown")
	

static func get_all_names() -> Array:
	return _factory_registry.keys()

static func register_command(name: String, factory_callable: Callable):
	_factory_registry[name] = factory_callable
	var sample_obj = factory_callable.call()
	_name_registry[sample_obj.get_script()] = name
