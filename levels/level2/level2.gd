extends LevelBase

var _is_finish: bool = false

@onready var coin: Coin = $Entities/Coin

func _init_level() -> void:
	coin.activated.connect(_finish)
	
func _is_goal_met() -> bool:
	return _is_finish
	
func _finish() -> void:
	_is_finish = true
	
