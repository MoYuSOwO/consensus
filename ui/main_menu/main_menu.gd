extends CanvasLayer
class_name MainMenu

@export var level_scene: PackedScene

@onready var main_buttons: Control = $ButtonContainer
@onready var level_panel: Control = $LevelSelectPanel
@onready var level_grid: GridContainer = $LevelSelectPanel/VBox/GridContainer

@onready var start_button: Button = $ButtonContainer/StartButton
@onready var quit_button: Button = $ButtonContainer/QuitButton
@onready var back_button: Button = $LevelSelectPanel/VBox/BackButton

var levels: Dictionary = {
	"Sector 01": "res://levels/level1/level1.tscn",
	"Sector 02": "res://levels/level2/level2.tscn",
	"Sector 03": "res://levels/level3/level3.tscn",
	"Sector 04": "res://levels/level4/level4.tscn"
}

func _ready() -> void:
	start_button.pressed.connect(toggle_level_select)
	back_button.pressed.connect(toggle_level_select)
	quit_button.pressed.connect(func(): get_tree().quit())

	level_panel.visible = false

	for level_name in levels.keys():
		var btn = Button.new()
		btn.text = level_name
		var path = levels[level_name]
		btn.pressed.connect(func(): start_level(path))
		level_grid.add_child(btn)

func toggle_level_select() -> void:
	main_buttons.visible = !main_buttons.visible
	level_panel.visible = !level_panel.visible

func start_level(path: String) -> void:
	if not level_scene:
		push_error("[MainMenu] You should have LevelScene first!")
		return
	
	var s = level_scene.instantiate() as LevelManager
	s.name = "LevelRoot"
	
	get_tree().root.add_child(s)
	get_tree().current_scene = s
	s.load_level(path)
	queue_free()
