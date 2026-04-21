extends RefCounted
class_name Util

enum Direction {
	DOWN = 0,
	LEFT = 1,
	UP = 2,
	RIGHT = 3
}
static var TYPE_DIRECTION = 40

static func direction_to_vector2i(dir: Direction) -> Vector2i:
	match dir:
		Direction.UP: return Vector2i(0, -1)
		Direction.DOWN: return Vector2i(0, 1)
		Direction.LEFT: return Vector2i(-1, 0)
		Direction.RIGHT: return Vector2i(1, 0)
		_:
			push_error("Unknown direction")
			return Vector2i.ZERO
			
static func direction_to_string(dir: Direction) -> String:
	match dir:
		Direction.UP: return "UP"
		Direction.DOWN: return "DOWN"
		Direction.LEFT: return "LEFT"
		Direction.RIGHT: return "RIGHT"
		_:
			push_error("Unknown direction")
			return "UNKNOWN"

static func direction_to_frame_id(dir: Direction) -> int:
	return dir as int
	
static func value_to_direction(dir_v: int) -> Direction:
	match dir_v:
		2: return Direction.UP
		0: return Direction.DOWN
		1: return Direction.LEFT
		3: return Direction.RIGHT
		_:
			push_error("Unknown direction")
			return Direction.DOWN


class ValueRange extends RefCounted:
	var value: float
	var min: float
	var max: float

	func _init(_v: float = 0.0, _min: float = NAN, _max: float = NAN):
		value = _v
		if _min != NAN && _max != NAN:
			min = _min
			max = _max
		else:
			min = _v
			max = _v
			
	func is_valid() -> bool:
		for v in [value, min, max]:
			if is_nan(v) or is_inf(v):
				return false
		return min <= value && value <= max
		
	func is_number() -> bool:
		return is_valid() && is_equal_approx(min, max)

	func plus(other) -> ValueRange:
		if other is ValueRange:
			return ValueRange.new(value + other.value, min + other.min, max + other.max)
		return ValueRange.new(value + other, min + other, max + other)

	func minus(other) -> ValueRange:
		if other is ValueRange:
			return ValueRange.new(value - other.value, min - other.min, max - other.max)
		return ValueRange.new(value - other, min - other, max - other)

	func multiply(other) -> ValueRange:
		if other is ValueRange:
			return ValueRange.new(value * other.value, min * other.min, max * other.max)
		return ValueRange.new(value * other, min * other, max * other)


class NetworkResult extends RefCounted:
	var dist: float
	var delay_ticks: ValueRange
	var strength: ValueRange
	var loss_prob: ValueRange


const GRID_SIZE: float = 64.0
const MIN_BASE_NETWORK_DELAY: float = 0.3
const MAX_BASE_NETWORK_DELAY: float = 0.7
const STRENGTH_PERFECT: float = 4.0
const STRENGTH_DEAD: float = 0.5
const CURVE_POWER: float = 1.5

static func get_random_network_delay() -> ValueRange:
	var val = randf_range(MIN_BASE_NETWORK_DELAY, MAX_BASE_NETWORK_DELAY)
	return ValueRange.new(
		TickManager.second_to_tick(val),
		TickManager.second_to_tick(MIN_BASE_NETWORK_DELAY),
		TickManager.second_to_tick(MAX_BASE_NETWORK_DELAY)
	)

static func get_loss_prob(strength: float) -> ValueRange:
	if strength >= STRENGTH_PERFECT:
		return ValueRange.new(0.0)
	elif strength <= STRENGTH_DEAD:
		return ValueRange.new(1.0)
	
	var t = (STRENGTH_PERFECT - strength) / (STRENGTH_PERFECT - STRENGTH_DEAD)
	var p = pow(t, CURVE_POWER)
	return ValueRange.new(p * randf_range(0.9, 1.1), p * 0.9, p * 1.1)

static func get_decrease_ratio(dist: float) -> ValueRange:
	var r = 1.0 / (1.0 + 0.3 * dist)
	return ValueRange.new(r * randf_range(0.9, 1.1), r * 0.9, r * 1.1)

static func get_random_robot_delay(min_time: float, max_time: float) -> ValueRange:
	return ValueRange.new(
		TickManager.second_to_tick(randf_range(min_time, max_time)),
		TickManager.second_to_tick(min_time),
		TickManager.second_to_tick(max_time)
	)

static func calculate_inf(from_robot: Robot, to_robot: Robot, command: RefCounted) -> NetworkResult:
	var res = NetworkResult.new()
	
	if to_robot == from_robot:
		res.dist = 0.0
		res.delay_ticks = get_random_network_delay()
		res.strength = ValueRange.new(command.send_strength)
		res.loss_prob = ValueRange.new(0.0)
	else:
		res.dist = from_robot.global_position.distance_to(to_robot.global_position) / GRID_SIZE
		
		var robot_delay = get_random_robot_delay(to_robot.min_send_delay, to_robot.max_send_delay)
		res.delay_ticks = get_random_network_delay().plus(robot_delay)
		
		res.strength = get_decrease_ratio(res.dist).multiply(command.send_strength)
		
		var min_loss = get_loss_prob(res.strength.min)
		var max_loss = get_loss_prob(res.strength.max)
		res.loss_prob = ValueRange.new(
			get_loss_prob(res.strength.value).value, 
			min(min_loss.min, max_loss.min), 
			max(min_loss.max, max_loss.max)
		)
		
	return res
