extends GridInteractableBase
class_name PressurePlate

var robot_on_plate: int = 0

func on_enter() -> void:
	robot_on_plate += 1
	if robot_on_plate == 1:
		activated.emit()
		modulate = Color.LIME_GREEN
		
func on_exit() -> void:
	robot_on_plate -= 1
	if robot_on_plate <= 0:
		robot_on_plate = 0
		deactivated.emit()
		modulate = Color.WHITE
