extends CanvasLayer
class_name GameOverlay

@onready var bg_dim: Control = $BackgroundDim
@onready var title_label: Label = $BackgroundDim/CenterContainer/VBox/TitleLabel
@onready var score_label: Label = $BackgroundDim/CenterContainer/VBox/ScoreLabel
@onready var pause_button: Button = $PauseButton
@onready var continue_btn: Button = $BackgroundDim/CenterContainer/VBox/ContinueBtn
@onready var retry_btn: Button = $BackgroundDim/CenterContainer/VBox/RetryBtn
@onready var menu_btn: Button = $BackgroundDim/CenterContainer/VBox/MenuBtn

@onready var level: LevelManager = get_parent().get_parent()

func _ready() -> void:
	bg_dim.visible = false
	
	pause_button.pressed.connect(pause)
	continue_btn.pressed.connect(unpause)
	
	retry_btn.pressed.connect(func():
		get_tree().paused = false
		level.reset()
	)
	
	menu_btn.pressed.connect(func():
		get_tree().paused = false
		get_tree().change_scene_to_file("res://ui/main_menu/main_menu.tscn")
	)

func pause() -> void:
	get_tree().paused = true
	title_label.text = "GAME PAUSED"
	title_label.self_modulate = Color.WHITE
	continue_btn.visible = true
	bg_dim.visible = true

func unpause() -> void:
	get_tree().paused = false
	bg_dim.visible = false

func on_game_ended(win: bool, s: String = "") -> void:
	get_tree().paused = true
	
	title_label.text = "MISSION ACCOMPLISHED" if win else "CONNECTION LOST"
	score_label.text = s
	continue_btn.visible = false
	bg_dim.visible = true
	
	title_label.self_modulate = Color.LIME_GREEN if win else Color.RED
