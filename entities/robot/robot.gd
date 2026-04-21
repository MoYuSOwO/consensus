extends Node2D
class_name Robot

@export var min_send_delay: float = 1.0
@export var max_send_delay: float = 3.0

const PLAYER_TEX = preload("res://entities/robot/player_robot.png")
const DEFAULT_TEX = preload("res://entities/robot/robot.png")

var id: String:
	get: return name

var grid_pos: Vector2i = Vector2i.ZERO
var is_busy: bool = false
var action_queue: Array[Command] = []
var facing_direction: Util.Direction = Util.Direction.DOWN

var map_layer: TileMapLayer
var network : NetworkManager
var ticker: TickManager
var entities: Dictionary[Vector2i, GridEntityBase] = {}

@onready var body: Sprite2D = $Body
@onready var progress_bar: ProgressBar = $ProgressBar
@onready var green_particle: CPUParticles2D = $GreenParticle
@onready var red_particle: CPUParticles2D = $RedParticle
@onready var blue_particle: CPUParticles2D = $BlueParticle
@onready var name_label: Label = $ColorRect/CenterContainer/NameLabel
@onready var leds: Array = [$HBoxContainer/ColorRect1, $HBoxContainer/ColorRect2, $HBoxContainer/ColorRect3]

func _ready() -> void:
	name_label.text = id

func init(_map: TileMapLayer, _network: NetworkManager, _ticker: TickManager, _entities: Dictionary[Vector2i, GridEntityBase], is_player: bool = false) -> void:
	map_layer = _map
	network = _network
	ticker = _ticker
	
	entities.merge(_entities)
	
	grid_pos = map_layer.local_to_map(position)
	position = map_layer.map_to_local(grid_pos)
	
	network.register_robot(self)
	ticker.tick_update.connect(_on_tick_update)
	
	body.texture = PLAYER_TEX if is_player else DEFAULT_TEX

func receive_command(cmd: Command) -> void:
	if action_queue.size() < 3:
		action_queue.append(cmd)
		blue_particle.emitting = true
		network.log_emitted.emit("[T+%s] command received: %s, Queue length: %s" % [TickManager.tick_to_second(ticker.get_current_tick()), cmd.to_string(), action_queue.size()], "#7ee787")
	else:
		red_particle.emitting = true
		network.log_emitted.emit("[T+%s] command package lost due to full queue: %s" % [TickManager.tick_to_second(ticker.get_current_tick()), cmd.to_string()], "#ff7b72")
	
	_update_queue_leds()

func _on_tick_update(_current_tick: int) -> void:
	if not is_busy and action_queue.size() > 0:
		is_busy = true
		var cmd = action_queue.pop_front()
		_update_queue_leds()
		cmd.execute(self)

func step(length: int) -> void:
	is_busy = true
	var direction_vec = Util.direction_to_vector2i(facing_direction)
	var actual_steps = 0
	
	for i in range(1, length + 1):
		var next_grid = grid_pos + direction_vec * i
		if _is_cell_wall(next_grid): break
		actual_steps = i

	var basic_duration = TickManager.tick_to_second(StepCommand.TICK_PER_STEP)
	var move_duration = max(actual_steps * basic_duration, basic_duration)
	
	_start_progress_bar(move_duration)
	
	var tween = create_tween()
	if actual_steps == 0:
		var bump_pos = position + Vector2(direction_vec) * 8.0
		tween.tween_property(self, "position", bump_pos, basic_duration * 0.3).set_trans(Tween.TRANS_QUAD)
		tween.tween_property(self, "position", position, basic_duration * 0.7).set_trans(Tween.TRANS_BOUNCE)
	else:
		var temp_grid = grid_pos
		for i in range(actual_steps):
			temp_grid += direction_vec
			var target_pixel = map_layer.map_to_local(temp_grid)
			tween.tween_property(self, "position", target_pixel, basic_duration)
			var step_grid = temp_grid
			tween.tween_callback(func(): _update_grid_position(step_grid))

	tween.finished.connect(func(): is_busy = false)

func rotate_to(direction: Util.Direction) -> void:
	is_busy = true
	facing_direction = direction
	
	var duration = TickManager.tick_to_second(RotateCommand.ROTATE_TICK)
	var durationA = duration / 4.0 * 3.0
	var durationB = duration - durationA
	
	_start_progress_bar(duration)
	
	var tween = create_tween()
	tween.tween_property(self, "scale", Vector2(1.2, 0.8), durationA)
	tween.tween_callback(func(): body.frame = Util.direction_to_frame_id(direction))
	tween.tween_property(self, "scale", Vector2.ONE, durationB).set_trans(Tween.TRANS_ELASTIC).set_ease(Tween.EASE_OUT)
	tween.finished.connect(func(): is_busy = false)
	
func wait(time: float) -> SceneTreeTimer:
	is_busy = true
	_start_progress_bar(time)
	var timer = get_tree().create_timer(time)
	timer.timeout.connect(func(): is_busy = false)
	return timer

func _is_cell_wall(p_grid_pos: Vector2i) -> bool:
	var tile_data = map_layer.get_cell_tile_data(p_grid_pos)
	var is_wall = tile_data != null and tile_data.get_custom_data("is_wall")
	var entity = entities.get(p_grid_pos)
	var is_blocked = entity != null and not entity.is_walkable()
	return is_wall or is_blocked

func _update_grid_position(new_grid: Vector2i) -> void:
	var old_entity = entities.get(grid_pos)
	if old_entity is GridInteractableBase:
		old_entity.on_exit()
		
	grid_pos = new_grid
	
	var new_entity = entities.get(grid_pos)
	if new_entity is GridInteractableBase:
		new_entity.on_enter()

func _start_progress_bar(duration: float) -> void:
	progress_bar.value = 0
	progress_bar.visible = true
	var tween = create_tween()
	tween.tween_property(progress_bar, "value", 100.0, duration)
	tween.finished.connect(func(): progress_bar.visible = false)

func _update_queue_leds() -> void:
	var count = action_queue.size()
	leds[0].color = Color.RED if count > 2 else Color.GREEN
	leds[1].color = Color.RED if count > 1 else Color.GREEN
	leds[2].color = Color.RED if count > 0 else Color.GREEN
