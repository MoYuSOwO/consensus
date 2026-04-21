extends GridEntityBase
class_name GridInteractableBase

signal activated
signal deactivated

func on_enter() -> void:
	push_error("on_enter() must be implemented in subclass: ", name)

func on_exit() -> void:
	pass
