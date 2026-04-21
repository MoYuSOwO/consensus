extends Node
class_name NetworkManager

signal log_emitted(message: String, color: Color)

var _registry: Dictionary[String, Robot] = {}
var _in_flight_packets: Array[Command] = []

func is_flight_empty() -> bool:
	return _in_flight_packets.is_empty()

func get_robot_ids() -> Array:
	return _registry.keys()

func get_robots() -> Array:
	return _registry.values()
	
func get_robot_by_id(id: String) -> Robot:
	return _registry.get(id)

func init(ticker: TickManager) -> void:
	ticker.tick_update.connect(_on_tick_update)

func register_robot(robot: Robot) -> void:
	_registry[robot.id] = robot
	
func send(command: Command) -> void:
	if not _registry.has(command.from_robot_id):
		push_error("[NetworkManager] Robot %s does not exist." % command.from_robot_id)
		return
	if not _registry.has(command.to_robot_id):
		push_error("[NetworkManager] Robot %s does not exist." % command.to_robot_id)
		return

	var from = _registry[command.from_robot_id]
	var to = _registry[command.to_robot_id]

	var v = Util.calculate_inf(to, from, command)

	command.calculated_arrival_tick = command.send_tick + v.delay_ticks.value
	command.calculated_arrival_strength = v.strength.value
	command.calculated_arrival_loss_ratio = v.loss_prob.value
	
	_in_flight_packets.append(command)

	print("[NetworkManager] %s -> %s. dist: %s, delay: %d Ticks, strength: %s -> %s" % \
		[command.from_robot_id, command.to_robot_id, v.dist, v.delay_ticks.value, command.send_strength, command.calculated_arrival_strength])
		
func send_all(commands: Array[Command]) -> void:
	for cmd in commands:
		send(cmd)
		

func _on_tick_update(current_tick: int) -> void:
	for command in _in_flight_packets:
		if command.send_tick == current_tick:
			if _registry.has(command.from_robot_id):
				_registry[command.from_robot_id].green_particle.emitting = true
			
			var time_str = TickManager.tick_to_second(current_tick)
			log_emitted.emit("[T+%s] command sent: %s" % [time_str, command.to_string()], "#cccccc")

	for i in range(_in_flight_packets.size() - 1, -1, -1):
		var command = _in_flight_packets[i]
		
		if command.calculated_arrival_tick <= current_tick:
			_in_flight_packets.remove_at(i)
			
			if _registry.has(command.to_robot_id):
				var robot = _registry[command.to_robot_id]
				var time_str = TickManager.tick_to_second(current_tick)
				
				if randf() > command.calculated_arrival_loss_ratio:
					robot.receive_command(command)
				else:
					robot.red_particle.emitting = true
					log_emitted.emit("[T+%s] command package lost due to connection: %s" % [time_str, command.to_string()], "#ff7b72")
