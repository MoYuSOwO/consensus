extends LevelBase

var _is_finish: bool = false

@onready var pressure_plate: PressurePlate = $Entities/PressurePlate
@onready var coin: Coin = $Entities/Coin
@onready var door: Door = $Entities/Door


func _init_level() -> void:
	coin.activated.connect(_finish)
	pressure_plate.activated.connect(door.open)
	pressure_plate.deactivated.connect(door.close)
	
func _is_goal_met() -> bool:
	return _is_finish
	
func _finish() -> void:
	_is_finish = true
	
