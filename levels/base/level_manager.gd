extends Node
class_name LevelManager

@export_file("*.tscn") var level_path: String = "res://levels/level1/level1.gd"
@export_file("*.tscn") var ui_path: String = "res://Levels/level_ui.tscn"

var _ticker: TickManager
var _network: NetworkManager
var _current_level: LevelBase
var _current_ui: LevelUI

func get_current_level() -> LevelBase:
	return _current_level

func _ready() -> void:
	pass

func load_level(p_level_path: String) -> void:
	print("[LevelManager] Start to load level: %s" % p_level_path)
	level_path = p_level_path

	for node in [_ticker, _network, _current_level, _current_ui]:
		if is_instance_valid(node):
			node.get_parent().remove_child(node)
			node.queue_free()

	_ticker = null
	_network = null
	_current_level = null
	_current_ui = null

	_instantiate.call_deferred(p_level_path)

func _instantiate(p_level_path: String) -> void:
	_ticker = TickManager.new()
	_ticker.name = "TickManager"
	add_child(_ticker)

	_network = NetworkManager.new()
	_network.name = "NetworkManager"
	add_child(_network)

	_network.init(_ticker)

	var level_scene = load(p_level_path)
	if not level_scene:
		push_error("[LevelManager] Fail to load level! Cannot find file: %s" % p_level_path)
		return
	
	_current_level = level_scene.instantiate() as LevelBase
	if not _current_level:
		push_error("[LevelManager] Root node of %s does not have a script attached that inherits from LevelBase!" % p_level_path)
		return
		
	_current_level.name = "CurrentLevel"
	add_child(_current_level)

	var ui_scene = load(ui_path)
	if not ui_scene:
		push_error("[LevelManager] Fail to load UI! Cannot find file: %s" % ui_path)
		return
		
	_current_ui = ui_scene.instantiate() as LevelUI
	if not _current_ui:
		push_error("[LevelManager] Root node of %s is not a LevelUI!" % ui_path)
		return
		
	_current_ui.name = "LevelUI"
	add_child(_current_ui)

	_current_ui.init(_network, _ticker, _current_level.main_robot_id)
	_network.log_emitted.connect(_current_ui.print_log)

	print("[LevelManager] Successfully loaded level and UI!")

func reset() -> void:
	load_level(level_path)
