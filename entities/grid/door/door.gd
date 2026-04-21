extends GridEntityBase
class_name Door

var is_open: bool = false

func is_walkable() -> bool:
	return is_open
	
@onready var body: Sprite2D = $Body

func open() -> void:
	if is_open:
		return
	is_open = true
	body.frame = 1
	
func close() -> void:
	if !is_open:
		return
	is_open = false
	body.frame = 0
