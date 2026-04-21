extends Node
class_name TickManager

const TICK_PER_SECOND: int = 10

static func get_tick_interval() -> float:
	return tick_to_second(1)

static func second_to_tick(second: float) -> int:
	return int(second * TICK_PER_SECOND)

static func tick_to_second(tick: int) -> float:
	return float(tick) / float(TICK_PER_SECOND)


var _current_tick: int = 0

func get_current_tick() -> int:
	return _current_tick;


var _is_running: bool = false

func is_running() -> bool:
	return _is_running;


var _timer: Timer


signal tick_update(tick: int)
signal ticking_started()
signal ticking_stopped()


func _ready() -> void:
	for child in get_children():
		child.queue_free()

	_timer = Timer.new()
	_timer.name = "TickTimer"
	_timer.wait_time = get_tick_interval()
	_timer.one_shot = false
	_timer.autostart = false
	
	_timer.timeout.connect(_on_timer_timeout)
	add_child(_timer)

	print("[TickManager] Timer is initialized, interval: %f, waiting to start..." % get_tick_interval())
	
func _on_timer_timeout() -> void:
	_current_tick += 1
	tick_update.emit(_current_tick)
	print("[TickManager] Tick: %d" % _current_tick)
	

func start_ticking() -> void:
	if _is_running:
		return
	
	_is_running = true
	_timer.start()
	ticking_started.emit()
	print("[TickManager] Timer is started, interval: %f" % get_tick_interval())

func stop_ticking() -> void:
	if not _is_running:
		return

	_is_running = false
	_timer.stop()
	ticking_stopped.emit()
	print("[TickManager] Timer is stopped.")

func reset_ticks() -> void:
	stop_ticking()
	_current_tick = 0
	print("[TickManager] Timer and Tick is reseted.")
