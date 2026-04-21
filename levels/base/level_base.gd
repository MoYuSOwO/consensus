extends Node
class_name LevelBase

signal win
signal fail

@export var total_signal_energy: float = 100.0
@export var main_robot_id: String = ""

const FAIL_WAIT_TICK: int = 20
var _fail_wait: int = 0

@onready var ticker: TickManager = get_parent().get_node("TickManager")
@onready var network: NetworkManager = get_parent().get_node("NetworkManager")
@onready var map_layer: TileMapLayer = $TileMapLayer
@onready var camera: Camera2D = $Camera2D

var robots: Dictionary[String, Robot] = {}
var entities_dict: Dictionary[Vector2i, GridEntityBase] = {}
var entities_list: Array[GridEntityBase] = []

func _ready() -> void:
	var entities_node = get_node("Entities")
	for child in entities_node.get_children():
		if child is GridEntityBase:
			entities_list.append(child)
			child.init(map_layer)
			entities_dict[child.grid_pos] = child

	var robots_node = get_node("Robots")
	for child in robots_node.get_children():
		if child is Robot:
			robots[child.id] = child
			child.init(map_layer, network, ticker, entities_dict, child.name == main_robot_id)

	ticker.tick_update.connect(_on_tick_start)
	
	_init_level()


func _init_level() -> void:
	pass

func _on_tick(_tick: int) -> void:
	pass

func _is_goal_met() -> bool:
	push_error("_is_goal_met() must be implemented in subclass!")
	return false


func _on_tick_start(tick: int) -> void:
	_check_status()
	_on_tick(tick)

func _check_status() -> void:
	if _is_goal_met():
		win.emit()
	elif _is_system_idle():
		fail.emit()

func _is_system_idle() -> bool:
	if not network.is_flight_empty():
		_fail_wait = 0
		return false
	
	for robot in robots.values():
		if not robot.action_queue.is_empty() or robot.is_busy:
			_fail_wait = 0
			return false
	
	_fail_wait += 1
	return _fail_wait > FAIL_WAIT_TICK
